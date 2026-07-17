export interface Lesson {
  id: number;
  name: string;
  description: string;
  durationSeconds: number;
  bunnyVideoId: string;
}

export interface Module {
  id: number;
  name: string;
  description: string;
  descriptionSub: string;
  imgCoverLink: string;
  bunnyCollectionId: string;
  lessons: Lesson[];
}

export interface Domain {
  id: number;
  title: string;
  description: string;
}

export interface Teacher {
  id: number;
  name: string;
  role: string;
  position: string;
  avatar: string;
  bio: string;
  instagramLink: string;
  linkedinLink: string;
  idAgivys: string;
}

export interface Category {
  id: number;
  name: string;
}

export interface Course {
  id: number;
  name: string;
  description: string;
  descriptionSub: string;
  active: boolean;
  level: string;
  priceSingle: number;
  imgCoverLink: string;
  bunnyLibraryId: string;
  workloadHours: number;
  modules: Module[];
  domains: Domain[];
  teachers: Teacher[];
  categories: Category[];
}
