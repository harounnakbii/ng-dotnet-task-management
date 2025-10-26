export interface ErrorMessages {
  [errorType: string]: {
    [fieldName: string]: string;
  } | string;
}