export type ApplicationStatus = 'active' | 'inactive' | 'pending';

const STATUS_LABELS: Record<ApplicationStatus, string> = {
  active: 'Ativa',
  inactive: 'Inativa',
  pending: 'Pendente'
};

export function applicationStatusLabel(status: string): string {
  return STATUS_LABELS[status as ApplicationStatus] ?? status;
}

export function applicationInitials(name: string): string {
  return name.trim().charAt(0).toUpperCase() || 'A';
}

export function applicationGradient(app: { primaryColor: string | null; secondaryColor: string | null }): string {
  const from = app.primaryColor || 'var(--accent-cyan)';
  const to = app.secondaryColor || 'var(--accent-purple)';
  return `linear-gradient(135deg, ${from}, ${to})`;
}

export function maskApiKey(apiKey: string | null | undefined): string {
  if (!apiKey) return '';
  if (apiKey.length <= 12) return apiKey;
  return `${apiKey.slice(0, 8)}${'•'.repeat(12)}${apiKey.slice(-4)}`;
}
