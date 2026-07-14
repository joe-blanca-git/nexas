export interface IPortalHomeData {
  latestCourse: ILatestCourse | null;
  latestNews: ILatestNews[];
  myLatestLesson?: IMyLatestLesson | null;
  myCoursesSummary?: IMyCoursesSummary | null;
}

export interface IMyLatestLesson {
  courseId: number;
  courseTitle: string;
  moduleId: number;
  moduleTitle: string;
  lessonId: number;
  lessonTitle: string;
  lessonOrder: number | null;
  thumbnail: string | null;
  lastViewedAt: string;
  progressPercentage: number;
}

export interface IMyCoursesSummary {
  totalCourses: number;
  completedCourses: number;
  coursesInProgress: number;
  overallProgress: number;
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
