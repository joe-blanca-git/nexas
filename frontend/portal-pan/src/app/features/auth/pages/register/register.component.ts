import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  steps = [
    { label: 'Dados pessoais', icon: 'fa-user' },
    { label: 'Endereço', icon: 'fa-map-marker-alt' }
  ];

  currentStep: number = 0;
  registerForm: FormGroup;
  isLoadingCep: boolean = false;
  submitted: boolean = false;

  showPassword: boolean = false;
  showConfirmPassword: boolean = false;

  constructor(private fb: FormBuilder, private router: Router) {
    this.registerForm = this.fb.group({
      // Dados Pessoais
      name: ['', [Validators.required, Validators.minLength(3)]],
      document: ['', [Validators.required, Validators.pattern(/^\d{11}$|^\d{14}$|^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$/)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required, 
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._\-#^()*+=\[\]{}|\\:<>,?~/`~]).{6,}$/)
      ]],
      confirmPassword: ['', [Validators.required]],
      birthDate: ['', [Validators.required]],

      // Endereço
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}-?\d{3}$/)]],
      street: ['', [Validators.required]],
      number: ['', [Validators.required]],
      complement: [''],
      neighborhood: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', [Validators.required, Validators.maxLength(2)]],
      addressDescription: ['Casa', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // Validador customizado para verificar se as senhas coincidem
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  // Getters para validações fáceis no HTML
  get f() { return this.registerForm.controls; }

  isStepValid(stepIndex: number): boolean {
    if (stepIndex === 0) {
      const personalFields = ['name', 'document', 'birthDate', 'email', 'password', 'confirmPassword'];
      return personalFields.every(field => this.registerForm.get(field)?.valid) && !this.registerForm.errors?.['mismatch'];
    } else if (stepIndex === 1) {
      const addressFields = ['zipCode', 'street', 'number', 'neighborhood', 'city', 'state', 'addressDescription'];
      return addressFields.every(field => this.registerForm.get(field)?.valid);
    }
    return false;
  }

  nextStep() {
    this.submitted = true;
    if (this.currentStep === 0) {
      const personalFields = ['name', 'document', 'birthDate', 'email', 'password'];
      let stepValid = true;
      personalFields.forEach(field => {
        const control = this.registerForm.get(field);
        control?.markAsTouched();
        if (control?.invalid) {
          stepValid = false;
        }
      });
      if (stepValid) {
        this.currentStep = 1;
        this.submitted = false;
      }
    }
  }

  prevStep() {
    if (this.currentStep > 0) {
      this.currentStep--;
      this.submitted = false;
    }
  }

  searchCep() {
    const cep = this.registerForm.get('zipCode')?.value?.replace(/\D/g, '');
    if (cep && cep.length === 8) {
      this.isLoadingCep = true;
      fetch(`https://viacep.com.br/ws/${cep}/json/`)
        .then(res => res.json())
        .then(data => {
          if (!data.erro) {
            this.registerForm.patchValue({
              street: data.logradouro,
              neighborhood: data.bairro,
              city: data.localidade,
              state: data.uf
            });
            ['street', 'neighborhood', 'city', 'state'].forEach(field => {
              this.registerForm.get(field)?.markAsTouched();
            });
          }
          this.isLoadingCep = false;
        })
        .catch(() => {
          this.isLoadingCep = false;
        });
    }
  }

  onSubmit() {
    this.submitted = true;
    this.registerForm.markAllAsTouched();

    if (this.registerForm.invalid) {
      return;
    }

    const formVal = this.registerForm.value;
    let isoBirthDate = formVal.birthDate;
    if (isoBirthDate) {
      try {
        isoBirthDate = new Date(isoBirthDate).toISOString();
      } catch (e) {
        console.error(e);
      }
    }

    const payload = {
      name: formVal.name,
      document: formVal.document.replace(/\D/g, ''),
      email: formVal.email,
      password: formVal.password,
      birthDate: isoBirthDate,
      addressDescription: formVal.addressDescription,
      zipCode: formVal.zipCode.replace(/\D/g, ''),
      street: formVal.street,
      number: formVal.number,
      complement: formVal.complement || '',
      neighborhood: formVal.neighborhood,
      city: formVal.city,
      state: formVal.state.toUpperCase()
    };

    console.log('API Payload:', payload);
    alert('Cadastro realizado com sucesso! (Veja o payload enviado no console do navegador)');
    this.router.navigate(['/auth/login']);
  }
}
