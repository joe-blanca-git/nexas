import { APP_BASE_HREF } from '@angular/common';
import { CommonEngine } from '@angular/ssr';
import express from 'express';
import { fileURLToPath } from 'node:url';
import { dirname, join, resolve } from 'node:path';
import bootstrap from './src/main.server';

export function app(): express.Express {
  const server = express();
  const serverDistFolder = dirname(fileURLToPath(import.meta.url));
  const browserDistFolder = resolve(serverDistFolder, '../browser');
  const indexHtml = join(serverDistFolder, 'index.server.html');

  const commonEngine = new CommonEngine();

  server.set('view engine', 'html');
  server.set('views', browserDistFolder);

  // CRÍTICO 1: Confia no Nginx. Evita que o Angular trave por causa de HTTPS -> HTTP.
  server.set('trust proxy', true);

  // CRÍTICO 2: Entrega o main.js e o styles.css perfeitamente.
  // 'index: false' impede que ele sirva o HTML em branco no lugar do SSR.
  server.use('/portal-pop', express.static(browserDistFolder, {
    maxAge: '1y',
    index: false 
  }));

  // CRÍTICO 3: SSR captura todas as rotas e manda para o Angular processar.
  server.get('**', (req, res, next) => {
    const { protocol, originalUrl, headers } = req;

    commonEngine
      .render({
        bootstrap,
        documentFilePath: indexHtml,
        url: `${protocol}://${headers.host}${originalUrl}`,
        publicPath: browserDistFolder,
        providers: [{ provide: APP_BASE_HREF, useValue: '/portal-pop/' }],
      })
      .then((html) => res.send(html))
      .catch((err) => next(err));
  });

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
