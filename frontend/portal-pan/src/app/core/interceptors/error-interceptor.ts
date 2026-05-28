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

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn,
): Observable<HttpEvent<any>> => {

  const router = inject(Router);
  const toastService = inject(ToastService);
  const stateUtil = inject(StateUtil);

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
          //erro de senha menor ou maior que o permitido.
          if (err.error?.errors?.Senha) {
            toastService.error(err.error.errors.Senha, 5000);
          };

          //outros erros
          if (err.error?.errors?.Mensagens) {
            toastService.error(err.error.errors.Mensagens, 5000);
          };

          if (err.error?.detail) {
            toastService.error(err.error.detail, 5000);
          };

          if (!err.error?.errors?.Senha || !err.error?.errors?.Mensagens || !err.error?.detail) {
            toastService.error('Erro interno, por favor solicite suporte!', 5000);
          };

          console.error(err)

        };

        if (err.status === 404) {
          toastService.error(err.error.detail, 5000);
        }

        //erro de autenticação
        if (err.status === 401) {
          toastService.error('Seu acesso expirou, por favor faça Login novamente!', 5000);
          stateUtil.clearState();
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

