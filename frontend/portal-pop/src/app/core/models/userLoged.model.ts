export class UserLogedModel {
  email?: string;
  name?: string;
  username?: string;
  id?: string;
  roles?: Role[];
  idPerson?: string;
}

interface Role {
    value: string;
    name: string;
}