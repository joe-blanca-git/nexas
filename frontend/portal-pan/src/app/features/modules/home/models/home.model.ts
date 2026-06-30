export interface IPortalHomeData {
  latestCourse: ILatestCourse | null;
  latestNews: ILatestNews[];
}

export interface ILatestCourse {
  id: number;
  title: string;
  description: string;
  rating: number;
  voteCount: number;
  headerImageUrl: string | null;
}

export interface ILatestNews {
  id: number;
  headerImageUrl: string | null;
  tags: string | null;
  title: string;
  subject: string;
  publishDate: string;
}
