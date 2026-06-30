//    =================================================================================================
//    SISTEMA.......: SIGAM-WEB
//    PROJETO.......: PORTAL SIGAM-WEB
//    OBJETO........: INTERCEPTADOR DE ERROS.
//    RESPONSÁVEL...: WESLEY-BONINI, HENE
//    AUTOR.........: JOEDER-BLANCA
//    DATA..........: 26/03/2026
//    =================================================================================================
//    PROPRIETÁRIO DO CÓDIGO FONTE: USINA ALTA MOGIANA S/A - AÇÚCAR E ÁLCOOL - COPYRIGHT - 2025
//    TODOS OS DIREITOS RESERVADOS. PROIBIDA A REPRODUÇÃO, DISTRIBUIÇÃO E QUALQUER USO NÃO AUTORIZADO 
//    DESTE CÓDIGO FONTE SEM AUTORIZAÇÃO ESCRITA.
//    =================================================================================================
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { StateUtil } from '../utils/UserState.util';
import { ToastService } from '../services/toast.service';
import { AuthUtil } from '../auth/auth.util';

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn,
): Observable<HttpEvent<any>> => {

  const router = inject(Router);
  const toastService = inject(ToastService);
  const stateUtil = inject(StateUtil);
  const authUtil = inject(AuthUtil);

  return next(req).pipe(
    map((event: HttpEvent<any>) => {
      if (event instanceof HttpResponse && event.body) {
        if (event.body.status === 3) {
          const msg = event.body.messages?.[0]?.text || 'Houve um erro no carregamento dos dados. Solicite Suporte!';
          toastService.error(msg, 5000);
          throw { status: 3, message: msg };
        }
      }
      return event;
    }),
    catchError((err: any) => {


      if (err instanceof HttpErrorResponse) {

        if (err.status === 400) {
          let handled = false;

          //erro de senha menor ou maior que o permitido.
          if (err.error?.errors?.Senha) {
            toastService.error(err.error.errors.Senha, 5000);
            handled = true;
          }

          //outros erros
          if (err.error?.errors?.Mensagens) {
            toastService.error(err.error.errors.Mensagens, 5000);
            handled = true;
          }

          if (err.error?.detail) {
            toastService.error(err.error.detail, 5000);
            handled = true;
          }

          // Erros com a chave message direto no corpo (ex: AuthController)
          if (err.error?.message) {
            toastService.error(err.error.message, 5000);
            handled = true;
          }

          // Erros do Identity (array de erros)
          if (Array.isArray(err.error)) {
            err.error.forEach((e: any) => {
              if (e.description) {
                toastService.error(e.description, 7000);
                handled = true;
              }
            });
          }

          if (!handled) {
            toastService.error('Erro de validação, verifique os dados informados!', 5000);
          }

          console.error(err);
        }

        if (err.status === 404) {
          toastService.error(err.error.detail, 5000);
        }

        //erro de autenticação
        if (err.status === 401) {
          const msg = err.error?.message || 'Seu acesso expirou, por favor faça Login novamente!';
          toastService.error(msg, 5000);
          stateUtil.clearState();
          authUtil.removeCookieAuth();
          sessionStorage.removeItem('nexas_user');
          router.navigate(['/auth/login']);
        }

        //erro: Proibido (Usuário conhecido, mas sem permissão)
        if (err.status === 403) {
          toastService.error('Você não tem permissão para acessar este recurso.', 5000);
          router.navigate(['/access-denied']);
        }

        //erro: Chamada não encontrada
        if (err.status === 404 || err.status === 405 || err.status === 500 && !err.error.messages[0].text) {
          toastService.error('Houve um erro no carregamento dos dados. Solicite Suporte!', 5000);
          console.error(err);
        }
        if (err.status === 404 || err.status === 405 || err.status === 500 && err.error.messages[0].text) {
          toastService.error(err.error.messages[0].text, 5000);
          console.error(err);
        }

        if (err.status === 504) {
          toastService.error('Falha ao obter os dados, Tente novamente mais tarde ou solicite suporte!', 5000);
        }

      }

      return throwError(() => err);
    }),
  );
};

