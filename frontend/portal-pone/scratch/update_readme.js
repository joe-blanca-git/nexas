const fs = require('fs');
const filePath = 'C:/Users/joeder-blanca/Documents/projetos-joe/git/nexas/frontend/portal-pon/README.md';
let content = fs.readFileSync(filePath, 'utf8');

// Append new section before the final footer
const insertIndex = content.lastIndexOf('---');
if (insertIndex !== -1) {
  const newSection = `
## 15. Padrão Ouro de Design (UI/UX Guidelines)

Para manter a interface com um aspecto visual consistente, moderno e de altíssima qualidade (o "Padrão Ouro"), todo novo componente e principalmente **Modais** devem seguir rigorosamente a arquitetura de design estruturada abaixo:

### 15.1 Modais: Estrutura Premium (Padrão Ouro)
Todo modal criado na aplicação deve possuir a seguinte estrutura de cabeçalho (\`modal-header\`). Esta estrutura inclui um fundo escuro elegante, bordas arredondadas e um ícone em marca d'água posicionado ao fundo, além de um círculo de destaque para o ícone principal.

**Código Template Padrão:**
\`\`\`html
<div class="modal fade" id="exemploModal" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered"> <!-- Use modal-md, modal-lg ou modal-xl conforme necessidade -->
    <div class="modal-content rounded-4 border-0 shadow-lg overflow-hidden d-flex flex-column">
      
      <!-- Premium Header -->
      <div class="modal-header bg-dark border-bottom-0 pb-3 pt-4 px-4 rounded-top-4 position-relative overflow-hidden flex-shrink-0">
        <!-- Ícone Marca D'água (Fundo) -->
        <div class="position-absolute top-0 end-0 p-3 opacity-25">
          <i class="fas fa-[ICONE] text-white" style="font-size: 8rem; transform: translate(20%, -30%);"></i>
        </div>
        
        <!-- Conteúdo do Header (Ícone Destaque + Títulos) -->
        <div class="d-flex align-items-center gap-3">
          <div class="bg-[COR] bg-opacity-10 text-[COR] rounded-circle d-flex align-items-center justify-content-center shadow-sm flex-shrink-0" style="width: 48px; height: 48px;">
            <i class="fas fa-[ICONE] fs-5"></i>
          </div>
          <div>
            <h5 class="modal-title fw-bolder text-white mb-0 fs-5">Título Principal</h5>
            <small class="text-white-50 fw-medium d-block" style="max-width: 300px;">Subtítulo ou instrução breve</small>
          </div>
        </div>
        
        <!-- Botão Fechar -->
        <button type="button" class="btn-close position-absolute top-0 end-0 mt-4 me-4" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>

      <!-- Corpo do Modal -->
      <div class="modal-body p-4 bg-secondary bg-opacity-10 custom-scrollbar overflow-auto">
        <!-- Conteúdo do Formulário / Lista -->
      </div>
      
    </div>
  </div>
</div>
\`\`\`

**Regras Essenciais para Modais:**
1. **Atributos de Layout:** Sempre mantenha \`position-relative overflow-hidden flex-shrink-0\` no \`modal-header\` para garantir que o ícone de marca d'água grande não "vaze" para fora da caixa do modal.
2. **Posicionamento da Marca D'água:** Use \`translate(20%, -30%)\` em fontes de tamanho \`8rem\` posicionado via \`top-0 end-0\`. Evite usar modificadores CSS como \`pointer-events: none\` ou \`z-index\` desnecessariamente, pois podem interferir na renderização de versões específicas do FontAwesome dependendo do motor do navegador.
3. **Cores Semânticas (\`[COR]\`):**
   - **Módulos:** \`primary\` (\`fa-layer-group\`)
   - **Aulas:** \`danger\` (\`fa-play-circle\`)
   - **Cursos/Educação:** \`success\` (\`fa-graduation-cap\`)
   - **Domínios/Web:** \`info\` (\`fa-globe\`)
   - **Professores/Pessoas:** \`primary\` ou coloração personalizada (\`fa-chalkboard-teacher\`)
   - **Exclusão/Avisos:** \`warning\` ou \`danger\` (\`fa-exclamation-triangle\`)
4. **Scroll e Altura:** Para modais muito grandes (como o Novo Curso em \`modal-xl\`), limite a altura (\`style="height: 90vh;"\`) e aplique \`overflow-auto\` no \`modal-body\` mantendo a estrutura de flex container, evitando assim que o modal inteiro cause scroll na tela principal.
5. **Harmonia Visual:** Empregue intensamente transparências do Bootstrap (\`bg-opacity-10\`) combinadas com cores sólidas nos textos/ícones internos para um aspecto visual refinado (Glassmorphism sutil).

`;
  
  content = content.substring(0, insertIndex) + newSection + content.substring(insertIndex);
  fs.writeFileSync(filePath, content, 'utf8');
  console.log('README.md updated with Padrão Ouro guidelines.');
} else {
  console.log('Could not find footer separator in README.md');
}
