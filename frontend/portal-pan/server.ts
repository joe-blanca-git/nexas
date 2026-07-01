import { APP_BASE_HREF } from '@angular/common';
import { CommonEngine } from '@angular/ssr';
import express from 'express';
import { fileURLToPath } from 'node:url';
import { dirname, join, resolve } from 'node:path';
import bootstrap from './src/main.server';

// =====================================================================
// O Nginx envia os pedidos SEM o prefixo /portal-pan/ (faz strip).
// Exemplo: /portal-pan/auth/login chega aqui como /auth/login
//
// Por isso o Express trabalha com rotas "normais" (sem sub-pasta),
// mas dizemos ao Angular SSR que o APP_BASE_HREF é /portal-pan/
// para que ele saiba reconstruir as URLs corretamente.
// =====================================================================

export function app(): express.Express {
  const server = express();
  const serverDistFolder = dirname(fileURLToPath(import.meta.url));
  const browserDistFolder = resolve(serverDistFolder, '../browser');
  const indexHtml = join(serverDistFolder, 'index.server.html');

  const commonEngine = new CommonEngine();

  server.set('view engine', 'html');
  server.set('views', browserDistFolder);

  // Serve static files from /browser (CSS, JS, images, etc.)
  server.get('**', express.static(browserDistFolder, {
    maxAge: '1y',
    index: 'index.html',
  }));

  // All regular routes use the Angular engine
  server.get('**', (req, res, next) => {
    const { protocol, originalUrl, headers } = req;

    commonEngine
      .render({
        bootstrap,
        documentFilePath: indexHtml,
        // Reconstrói a URL completa com /portal-pan para o Angular SSR
        url: `${protocol}://${headers.host}/portal-pan${originalUrl}`,
        publicPath: browserDistFolder,
        // Diz ao Angular que a base é /portal-pan/ (igual ao <base href> do index.html)
        providers: [{ provide: APP_BASE_HREF, useValue: '/portal-pan/' }],
      })
      .then((html) => res.send(html))
      .catch((err) => next(err));
  });

  return server;
}

function run(): void {
  const port = process.env['PORT'] || 4000;

  // Start up the Node server
  const server = app();
  server.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

run();
