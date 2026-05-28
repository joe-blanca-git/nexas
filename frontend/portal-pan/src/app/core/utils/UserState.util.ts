
import { inject, Injectable } from '@angular/core';
import { createAction, createReducer, on, props, createFeatureSelector, createSelector, Store } from '@ngrx/store';
import { UserLogedModel } from '../models/userLoged.model';

/**
 * ACTIONS
 */
export const setUser = createAction(
    '[User Profile] Set User Data',
    props<{ userData: UserLogedModel }>()
);

export const clearUser = createAction('[User Profile] Clear User Data');

export const updateMonthsArchived = createAction(
    '[User Profile] Update Months Archived',
    props<{ months: number }>()
);

/**
 * STATE INTERFACE
 */
export interface UserState {
    profile: {
        email?: string;
        name?: string;
        username?: string;
        id?: string;
        roles?: Role[];
        idPerson?: string;
    } | null;
}

interface Role {
    value: string;
    name: string;
}

export const initialUserState: UserState = {
    profile: null
};

/**
 * REDUCER
 */
export const userProfileReducer = createReducer(
    initialUserState,
    on(setUser, (state, { userData }) => ({
        ...state,
        profile: {
            email: userData?.email || userData.email,
            name: userData?.name || userData.name,
            username: userData?.username || userData.username,
            id: userData?.id || userData.id,
            roles: userData?.roles || userData.roles || [],
            idPerson: userData?.idPerson || userData.idPerson,
        }
    })),
    on(clearUser, () => initialUserState),
);

/**
 * SELECTORS
 */
export const selectUserState = createFeatureSelector<UserState>('userProfile');
export const selectUserProfile = createSelector(selectUserState,(state: UserState) => state?.profile);


@Injectable({
    providedIn: 'root'
})
export class StateUtil {
    private store = inject(Store);

    public saveUser(response: any): void {
        const userData: UserLogedModel = {
            email: response?.person?.email || response.user?.email || null,
            name: response?.person?.name || response.user?.name || null,
            id: response?.person?.id || response.user?.id || null,
            roles: response.user?.roles || [],
            idPerson: response?.person?.id || null,
        };

        this.store.dispatch(setUser({ userData }));
    }

    public getUser() {
        return this.store.select(selectUserProfile);
    }

    public clearState(): void {
        this.store.dispatch(clearUser());
    }

}