const SCRIPT_ID = 'google-identity-script';
const SCRIPT_SRC = 'https://accounts.google.com/gsi/client';

let loadPromise: Promise<void> | null = null;

/**
 * Carrega o script do Google Identity Services sob demanda e resolve quando ele está
 * pronto — em vez de depender de uma tag <script> estática no index.html (carregada
 * "async defer") e checar `typeof google !== 'undefined'` uma única vez no ngOnInit.
 * Essa checagem única é uma condição de corrida: se o script ainda não tiver terminado
 * de carregar nesse instante (rede lenta, cache frio, hidratação do SSR), o botão do
 * Google nunca aparece, sem nenhum erro visível — parece "sumir" de forma intermitente.
 */
export function loadGoogleIdentityScript(): Promise<void> {
  if (typeof window === 'undefined') {
    return Promise.resolve(); // SSR: não faz nada, o botão só é renderizado no cliente
  }

  if ((window as any).google?.accounts?.id) {
    return Promise.resolve();
  }

  if (loadPromise) {
    return loadPromise;
  }

  loadPromise = new Promise((resolve, reject) => {
    const existing = document.getElementById(SCRIPT_ID) as HTMLScriptElement | null;
    if (existing) {
      existing.addEventListener('load', () => resolve());
      existing.addEventListener('error', () => reject(new Error('Falha ao carregar o script do Google Identity.')));
      return;
    }

    const script = document.createElement('script');
    script.id = SCRIPT_ID;
    script.src = SCRIPT_SRC;
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error('Falha ao carregar o script do Google Identity.'));
    document.head.appendChild(script);
  });

  return loadPromise;
}
