import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { IconComponent } from '../../shared/icon/icon';

@Component({
  selector: 'app-auth-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss'
})
export class AuthPageComponent {
  readonly mode = input<'login' | 'register'>('login');

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isRegister = computed(() => this.mode() === 'register');
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  fullName = '';
  email = '';
  phone = '';
  password = '';
  confirmPassword = '';
  marketingOptIn = true;
  acceptedTerms = false;

  submit(): void {
    this.errorMessage.set(null);

    if (this.isRegister()) {
      this.register();
    } else {
      this.login();
    }
  }

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  useDemo(role: 'customer' | 'admin'): void {
    if (role === 'admin') {
      this.email = 'admin@noor.store';
      this.password = 'Admin@12345';
    } else {
      this.email = 'demo@noor.store';
      this.password = 'Demo@12345';
    }
    this.errorMessage.set(null);
  }

  private login(): void {
    if (!this.email.trim() || !this.password) {
      this.errorMessage.set('Please enter your email and password.');
      return;
    }

    this.submitting.set(true);
    this.auth.login({ email: this.email.trim(), password: this.password }).subscribe({
      next: (response) => {
        this.submitting.set(false);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.router.navigate([response.user.role === 'Admin' ? '/admin' : '/account']);
        }
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(
          error instanceof Error ? error.message : 'We could not sign you in.'
        );
      }
    });
  }

  private register(): void {
    if (!this.fullName.trim() || !this.email.trim() || !this.password) {
      this.errorMessage.set('Please complete your name, email, and password.');
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.errorMessage.set('Your passwords do not match.');
      return;
    }
    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(this.password)) {
      this.errorMessage.set(
        'Use at least 8 characters, with uppercase, lowercase, and a number.'
      );
      return;
    }
    if (!this.acceptedTerms) {
      this.errorMessage.set('Please accept the terms and privacy notice to continue.');
      return;
    }

    this.submitting.set(true);
    this.auth
      .register({
        fullName: this.fullName.trim(),
        email: this.email.trim(),
        password: this.password,
        phone: this.phone.trim() || null,
        marketingOptIn: this.marketingOptIn
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/account']);
        },
        error: (error: unknown) => {
          this.submitting.set(false);
          this.errorMessage.set(
            error instanceof Error ? error.message : 'We could not create your account.'
          );
        }
      });
  }
}
