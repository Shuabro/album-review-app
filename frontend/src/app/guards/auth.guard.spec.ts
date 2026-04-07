import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;
  const dummyRoute = {} as ActivatedRouteSnapshot;
  const dummyState = {} as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        AuthService,
      ]
    });
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  it('should allow access when the user is logged in', () => {
    spyOn(authService, 'isLoggedIn').and.returnValue(true);
    const result = TestBed.runInInjectionContext(() =>
      authGuard(dummyRoute, dummyState)
    );
    expect(result).toBe(true);
  });

  it('should redirect to /login when the user is not logged in', () => {
    spyOn(authService, 'isLoggedIn').and.returnValue(false);
    const result = TestBed.runInInjectionContext(() =>
      authGuard(dummyRoute, dummyState)
    );
    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
