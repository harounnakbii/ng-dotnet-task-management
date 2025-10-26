import { AuthConfig } from "angular-oauth2-oidc";

export const environment = {
  apiUrl:'https://localhost:5002/api', 
  oidc: {
    issuer: 'https://localhost:5001',
    redirectUri: "http://localhost:4200",
    clientId: 'angular-client',
    responseType: 'code',
    scope: 'openid profile email offline_access taskapi',
    showDebugInformation: true,
    strictDiscoveryDocumentValidation: false,
    useSilentRefresh: false,
    tokenEndpoint : 'https://localhost:5001/connect/token'
  } as AuthConfig
};