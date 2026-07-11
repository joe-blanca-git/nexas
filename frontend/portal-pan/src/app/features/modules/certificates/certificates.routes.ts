import { Routes } from "@angular/router";
import { CertificatesHomeComponent } from "./pages/certificates-home/certificates-home.component";

export const certificatesRoutes: Routes = [
    {
        path: '',
        component: CertificatesHomeComponent,
        title: 'Certificados',
    }
];