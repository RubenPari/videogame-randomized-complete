import { ConnectorConfig, DataConnect, QueryRef, QueryPromise, MutationRef, MutationPromise } from 'firebase/data-connect';

export const connectorConfig: ConnectorConfig;

export type TimestampString = string;
export type UUIDString = string;
export type Int64String = string;
export type DateString = string;




export interface AddGameToMyLibraryData {
  userGame_insert: UserGame_Key;
}

export interface AddGameToMyLibraryVariables {
  gameId: UUIDString;
  status: string;
  notes?: string | null;
  completionStatus?: string | null;
}

export interface GameGenre_Key {
  id: UUIDString;
  __typename?: 'GameGenre_Key';
}

export interface GamePlatform_Key {
  id: UUIDString;
  __typename?: 'GamePlatform_Key';
}

export interface Game_Key {
  id: UUIDString;
  __typename?: 'Game_Key';
}

export interface GetMyGamesData {
  userGames: ({
    status: string;
    notes?: string | null;
    completionStatus?: string | null;
    lastPlayedAt?: TimestampString | null;
    game: {
      title: string;
      releaseYear: number;
      genres?: string[] | null;
    };
  })[];
}

export interface ListAllGamesData {
  games: ({
    id: UUIDString;
    title: string;
    summary?: string | null;
    releaseYear: number;
    genres?: string[] | null;
    platforms?: string[] | null;
    avgPlaytimeHours?: number | null;
  } & Game_Key)[];
}

export interface UpdateGameStatusInMyLibraryData {
  userGame_update?: UserGame_Key | null;
}

export interface UpdateGameStatusInMyLibraryVariables {
  gameId: UUIDString;
  status: string;
  notes?: string | null;
  completionStatus?: string | null;
}

export interface UserGame_Key {
  userId: UUIDString;
  gameId: UUIDString;
  __typename?: 'UserGame_Key';
}

export interface User_Key {
  id: UUIDString;
  __typename?: 'User_Key';
}

interface ListAllGamesRef {
  /* Allow users to create refs without passing in DataConnect */
  (): QueryRef<ListAllGamesData, undefined>;
  /* Allow users to pass in custom DataConnect instances */
  (dc: DataConnect): QueryRef<ListAllGamesData, undefined>;
  operationName: string;
}
export const listAllGamesRef: ListAllGamesRef;

export function listAllGames(): QueryPromise<ListAllGamesData, undefined>;
export function listAllGames(dc: DataConnect): QueryPromise<ListAllGamesData, undefined>;

interface GetMyGamesRef {
  /* Allow users to create refs without passing in DataConnect */
  (): QueryRef<GetMyGamesData, undefined>;
  /* Allow users to pass in custom DataConnect instances */
  (dc: DataConnect): QueryRef<GetMyGamesData, undefined>;
  operationName: string;
}
export const getMyGamesRef: GetMyGamesRef;

export function getMyGames(): QueryPromise<GetMyGamesData, undefined>;
export function getMyGames(dc: DataConnect): QueryPromise<GetMyGamesData, undefined>;

interface AddGameToMyLibraryRef {
  /* Allow users to create refs without passing in DataConnect */
  (vars: AddGameToMyLibraryVariables): MutationRef<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;
  /* Allow users to pass in custom DataConnect instances */
  (dc: DataConnect, vars: AddGameToMyLibraryVariables): MutationRef<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;
  operationName: string;
}
export const addGameToMyLibraryRef: AddGameToMyLibraryRef;

export function addGameToMyLibrary(vars: AddGameToMyLibraryVariables): MutationPromise<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;
export function addGameToMyLibrary(dc: DataConnect, vars: AddGameToMyLibraryVariables): MutationPromise<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;

interface UpdateGameStatusInMyLibraryRef {
  /* Allow users to create refs without passing in DataConnect */
  (vars: UpdateGameStatusInMyLibraryVariables): MutationRef<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;
  /* Allow users to pass in custom DataConnect instances */
  (dc: DataConnect, vars: UpdateGameStatusInMyLibraryVariables): MutationRef<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;
  operationName: string;
}
export const updateGameStatusInMyLibraryRef: UpdateGameStatusInMyLibraryRef;

export function updateGameStatusInMyLibrary(vars: UpdateGameStatusInMyLibraryVariables): MutationPromise<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;
export function updateGameStatusInMyLibrary(dc: DataConnect, vars: UpdateGameStatusInMyLibraryVariables): MutationPromise<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;

