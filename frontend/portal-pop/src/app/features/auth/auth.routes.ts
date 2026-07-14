import { Routes } from "@angular/router";
import { AuthAppComponent } from "./auth.app.component";
import { LoginComponent } from "./pages/login/login.component";
import { RecoveryPasswordComponent } from "./pages/recovery-password/recovery-password.component";
import { RegisterComponent } from "./pages/register/register.component";
import { UpdatePasswordComponent } from "./pages/update-password/update-password.component";
import { AuthGuardService } from "../../core/guards/auth.guard";

export const AUTH_ROUTES: Routes = [
    {
        path: '',
        component: AuthAppComponent,
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'login',
            },
            {
                path: 'login',
                component: LoginComponent,
                canActivate: [AuthGuardService],
            },
            {
                path: 'recovery-password',
                component: RecoveryPasswordComponent,
                canActivate: [AuthGuardService],
            },
            {
                path: 'reset-password',
                component: UpdatePasswordComponent,
                canActivate: [AuthGuardService],
            },
            {
                path: 'register',
                component: RegisterComponent,
                canActivate: [AuthGuardService],
            },
        ]
    }
];