import { Injectable, signal } from '@angular/core';
import { UserLogedModel } from '../models/userLoged.model';

@Injectable({
  providedIn: 'root'
})
export class StateUtil {
  // Using Angular Signals for modern state management
  private userSignal = signal<UserLogedModel | null>(null);

  get user() {
    return this.userSignal();
  }

  saveUser(user: UserLogedModel) {
    this.userSignal.set(user);
  }

  updateUser(partial: Partial<UserLogedModel>) {
    const current = this.userSignal();
    if (!current) return;
    this.userSignal.set({ ...current, ...partial });
  }

  clearState() {
    this.userSignal.set(null);
  }
}
