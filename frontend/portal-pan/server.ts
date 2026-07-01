import { APP_BASE_HREF } from '@angular/common';
import { CommonEngine } from '@angular/ssr';
import express from 'express';
import { fileURLToPath } from 'node:url';
import { dirname, join, resolve } from 'node:path';
import bootstrap from './src/main.server';

// =====================================================================
// SOLUÇÃO DEFINITIVA PARA ANGULAR SSR EM SUB-PASTA (/portal-pan)
//
// Estratégia:
// 1. O Nginx NÃO faz strip do prefixo (proxy_pass SEM barra no final)
// 2. O Node recebe a URL completa: /portal-pan/auth/login
// 3. O express.static é montado em /portal-pan para servir JS/CSS/imgs
// 4. O SSR handler trata todas as rotas sob /portal-pan/*
// 5. O APP_BASE_HREF é hardcoded como /portal-pan/
// =====================================================================

const BASE_PATH = '/portal-pan';

export function app(): express.Express {
  const server = express();
  const serverDistFolder = dirname(fileURLToPath(import.meta.url));
  const browserDistFolder = resolve(serverDistFolder, '../browser');
  const indexHtml = join(serverDistFolder, 'index.server.html');

  const commonEngine = new CommonEngine();

  server.set('view engine', 'html');
  server.set('views', browserDistFolder);

  // ---------------------------------------------------------------
  // 1) Servir ficheiros estáticos (JS, CSS, imagens, fontes, etc.)
  //    Montado em /portal-pan, o Express automaticamente faz strip
  //    do prefixo para encontrar os ficheiros na pasta do build.
  //    Ex: /portal-pan/main.abc.js → procura main.abc.js em browserDistFolder
  //
  //    index: false → Não servir index.html automaticamente para /
  //    Queremos que o SSR trate TODAS as rotas, incluindo a raiz.
  // ---------------------------------------------------------------
  server.use(BASE_PATH, express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
  }));

  // ---------------------------------------------------------------
  // 2) SSR Handler - Trata TODAS as rotas sob /portal-pan
  //    Usa uma função comum para evitar duplicação.
  // ---------------------------------------------------------------
  const ssrHandler: express.RequestHandler = (req, res, next) => {
    const { protocol, originalUrl, headers } = req;

    commonEngine
      .render({
        bootstrap,
        documentFilePath: indexHtml,
        url: `${protocol}://${headers.host}${originalUrl}`,
        publicPath: browserDistFolder,
        providers: [{ provide: APP_BASE_HREF, useValue: `${BASE_PATH}/` }],
      })
      .then((html) => res.send(html))
      .catch((err) => next(err));
  };

  // Rota exata: /portal-pan (sem barra)
  server.get(BASE_PATH, ssrHandler);

  // Todas as sub-rotas: /portal-pan/*, /portal-pan/auth/login, etc.
  server.get(`${BASE_PATH}/*`, ssrHandler);

  return server;
}

function run(): void {
  const port = process.env['PORT'] || 4000;

  const server = app();
  server.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

run();
