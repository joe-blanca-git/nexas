import { Routes } from "@angular/router";
import { CertificatesHomeComponent } from "./pages/certificates-home/certificates-home.component";
import { CertificateViewerComponent } from "./pages/certificate-viewer/certificate-viewer.component";

export const certificatesRoutes: Routes = [
    {
        path: '',
        component: CertificatesHomeComponent,
        title: 'Certificados',
    },
    {
        path: 'viewer/:code',
        component: CertificateViewerComponent,
        title: 'Certificado',
    }
];