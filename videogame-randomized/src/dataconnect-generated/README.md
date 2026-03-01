# Generated TypeScript README
This README will guide you through the process of using the generated JavaScript SDK package for the connector `example`. It will also provide examples on how to use your generated SDK to call your Data Connect queries and mutations.

***NOTE:** This README is generated alongside the generated SDK. If you make changes to this file, they will be overwritten when the SDK is regenerated.*

# Table of Contents
- [**Overview**](#generated-javascript-readme)
- [**Accessing the connector**](#accessing-the-connector)
  - [*Connecting to the local Emulator*](#connecting-to-the-local-emulator)
- [**Queries**](#queries)
  - [*ListAllGames*](#listallgames)
  - [*GetMyGames*](#getmygames)
- [**Mutations**](#mutations)
  - [*AddGameToMyLibrary*](#addgametomylibrary)
  - [*UpdateGameStatusInMyLibrary*](#updategamestatusinmylibrary)

# Accessing the connector
A connector is a collection of Queries and Mutations. One SDK is generated for each connector - this SDK is generated for the connector `example`. You can find more information about connectors in the [Data Connect documentation](https://firebase.google.com/docs/data-connect#how-does).

You can use this generated SDK by importing from the package `@dataconnect/generated` as shown below. Both CommonJS and ESM imports are supported.

You can also follow the instructions from the [Data Connect documentation](https://firebase.google.com/docs/data-connect/web-sdk#set-client).

```typescript
import { getDataConnect } from 'firebase/data-connect';
import { connectorConfig } from '@dataconnect/generated';

const dataConnect = getDataConnect(connectorConfig);
```

## Connecting to the local Emulator
By default, the connector will connect to the production service.

To connect to the emulator, you can use the following code.
You can also follow the emulator instructions from the [Data Connect documentation](https://firebase.google.com/docs/data-connect/web-sdk#instrument-clients).

```typescript
import { connectDataConnectEmulator, getDataConnect } from 'firebase/data-connect';
import { connectorConfig } from '@dataconnect/generated';

const dataConnect = getDataConnect(connectorConfig);
connectDataConnectEmulator(dataConnect, 'localhost', 9399);
```

After it's initialized, you can call your Data Connect [queries](#queries) and [mutations](#mutations) from your generated SDK.

# Queries

There are two ways to execute a Data Connect Query using the generated Web SDK:
- Using a Query Reference function, which returns a `QueryRef`
  - The `QueryRef` can be used as an argument to `executeQuery()`, which will execute the Query and return a `QueryPromise`
- Using an action shortcut function, which returns a `QueryPromise`
  - Calling the action shortcut function will execute the Query and return a `QueryPromise`

The following is true for both the action shortcut function and the `QueryRef` function:
- The `QueryPromise` returned will resolve to the result of the Query once it has finished executing
- If the Query accepts arguments, both the action shortcut function and the `QueryRef` function accept a single argument: an object that contains all the required variables (and the optional variables) for the Query
- Both functions can be called with or without passing in a `DataConnect` instance as an argument. If no `DataConnect` argument is passed in, then the generated SDK will call `getDataConnect(connectorConfig)` behind the scenes for you.

Below are examples of how to use the `example` connector's generated functions to execute each query. You can also follow the examples from the [Data Connect documentation](https://firebase.google.com/docs/data-connect/web-sdk#using-queries).

## ListAllGames
You can execute the `ListAllGames` query using the following action shortcut function, or by calling `executeQuery()` after calling the following `QueryRef` function, both of which are defined in [dataconnect-generated/index.d.ts](./index.d.ts):
```typescript
listAllGames(): QueryPromise<ListAllGamesData, undefined>;

interface ListAllGamesRef {
  ...
  /* Allow users to create refs without passing in DataConnect */
  (): QueryRef<ListAllGamesData, undefined>;
}
export const listAllGamesRef: ListAllGamesRef;
```
You can also pass in a `DataConnect` instance to the action shortcut function or `QueryRef` function.
```typescript
listAllGames(dc: DataConnect): QueryPromise<ListAllGamesData, undefined>;

interface ListAllGamesRef {
  ...
  (dc: DataConnect): QueryRef<ListAllGamesData, undefined>;
}
export const listAllGamesRef: ListAllGamesRef;
```

If you need the name of the operation without creating a ref, you can retrieve the operation name by calling the `operationName` property on the listAllGamesRef:
```typescript
const name = listAllGamesRef.operationName;
console.log(name);
```

### Variables
The `ListAllGames` query has no variables.
### Return Type
Recall that executing the `ListAllGames` query returns a `QueryPromise` that resolves to an object with a `data` property.

The `data` property is an object of type `ListAllGamesData`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:
```typescript
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
```
### Using `ListAllGames`'s action shortcut function

```typescript
import { getDataConnect } from 'firebase/data-connect';
import { connectorConfig, listAllGames } from '@dataconnect/generated';


// Call the `listAllGames()` function to execute the query.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await listAllGames();

// You can also pass in a `DataConnect` instance to the action shortcut function.
const dataConnect = getDataConnect(connectorConfig);
const { data } = await listAllGames(dataConnect);

console.log(data.games);

// Or, you can use the `Promise` API.
listAllGames().then((response) => {
  const data = response.data;
  console.log(data.games);
});
```

### Using `ListAllGames`'s `QueryRef` function

```typescript
import { getDataConnect, executeQuery } from 'firebase/data-connect';
import { connectorConfig, listAllGamesRef } from '@dataconnect/generated';


// Call the `listAllGamesRef()` function to get a reference to the query.
const ref = listAllGamesRef();

// You can also pass in a `DataConnect` instance to the `QueryRef` function.
const dataConnect = getDataConnect(connectorConfig);
const ref = listAllGamesRef(dataConnect);

// Call `executeQuery()` on the reference to execute the query.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await executeQuery(ref);

console.log(data.games);

// Or, you can use the `Promise` API.
executeQuery(ref).then((response) => {
  const data = response.data;
  console.log(data.games);
});
```

## GetMyGames
You can execute the `GetMyGames` query using the following action shortcut function, or by calling `executeQuery()` after calling the following `QueryRef` function, both of which are defined in [dataconnect-generated/index.d.ts](./index.d.ts):
```typescript
getMyGames(): QueryPromise<GetMyGamesData, undefined>;

interface GetMyGamesRef {
  ...
  /* Allow users to create refs without passing in DataConnect */
  (): QueryRef<GetMyGamesData, undefined>;
}
export const getMyGamesRef: GetMyGamesRef;
```
You can also pass in a `DataConnect` instance to the action shortcut function or `QueryRef` function.
```typescript
getMyGames(dc: DataConnect): QueryPromise<GetMyGamesData, undefined>;

interface GetMyGamesRef {
  ...
  (dc: DataConnect): QueryRef<GetMyGamesData, undefined>;
}
export const getMyGamesRef: GetMyGamesRef;
```

If you need the name of the operation without creating a ref, you can retrieve the operation name by calling the `operationName` property on the getMyGamesRef:
```typescript
const name = getMyGamesRef.operationName;
console.log(name);
```

### Variables
The `GetMyGames` query has no variables.
### Return Type
Recall that executing the `GetMyGames` query returns a `QueryPromise` that resolves to an object with a `data` property.

The `data` property is an object of type `GetMyGamesData`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:
```typescript
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
```
### Using `GetMyGames`'s action shortcut function

```typescript
import { getDataConnect } from 'firebase/data-connect';
import { connectorConfig, getMyGames } from '@dataconnect/generated';


// Call the `getMyGames()` function to execute the query.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await getMyGames();

// You can also pass in a `DataConnect` instance to the action shortcut function.
const dataConnect = getDataConnect(connectorConfig);
const { data } = await getMyGames(dataConnect);

console.log(data.userGames);

// Or, you can use the `Promise` API.
getMyGames().then((response) => {
  const data = response.data;
  console.log(data.userGames);
});
```

### Using `GetMyGames`'s `QueryRef` function

```typescript
import { getDataConnect, executeQuery } from 'firebase/data-connect';
import { connectorConfig, getMyGamesRef } from '@dataconnect/generated';


// Call the `getMyGamesRef()` function to get a reference to the query.
const ref = getMyGamesRef();

// You can also pass in a `DataConnect` instance to the `QueryRef` function.
const dataConnect = getDataConnect(connectorConfig);
const ref = getMyGamesRef(dataConnect);

// Call `executeQuery()` on the reference to execute the query.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await executeQuery(ref);

console.log(data.userGames);

// Or, you can use the `Promise` API.
executeQuery(ref).then((response) => {
  const data = response.data;
  console.log(data.userGames);
});
```

# Mutations

There are two ways to execute a Data Connect Mutation using the generated Web SDK:
- Using a Mutation Reference function, which returns a `MutationRef`
  - The `MutationRef` can be used as an argument to `executeMutation()`, which will execute the Mutation and return a `MutationPromise`
- Using an action shortcut function, which returns a `MutationPromise`
  - Calling the action shortcut function will execute the Mutation and return a `MutationPromise`

The following is true for both the action shortcut function and the `MutationRef` function:
- The `MutationPromise` returned will resolve to the result of the Mutation once it has finished executing
- If the Mutation accepts arguments, both the action shortcut function and the `MutationRef` function accept a single argument: an object that contains all the required variables (and the optional variables) for the Mutation
- Both functions can be called with or without passing in a `DataConnect` instance as an argument. If no `DataConnect` argument is passed in, then the generated SDK will call `getDataConnect(connectorConfig)` behind the scenes for you.

Below are examples of how to use the `example` connector's generated functions to execute each mutation. You can also follow the examples from the [Data Connect documentation](https://firebase.google.com/docs/data-connect/web-sdk#using-mutations).

## AddGameToMyLibrary
You can execute the `AddGameToMyLibrary` mutation using the following action shortcut function, or by calling `executeMutation()` after calling the following `MutationRef` function, both of which are defined in [dataconnect-generated/index.d.ts](./index.d.ts):
```typescript
addGameToMyLibrary(vars: AddGameToMyLibraryVariables): MutationPromise<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;

interface AddGameToMyLibraryRef {
  ...
  /* Allow users to create refs without passing in DataConnect */
  (vars: AddGameToMyLibraryVariables): MutationRef<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;
}
export const addGameToMyLibraryRef: AddGameToMyLibraryRef;
```
You can also pass in a `DataConnect` instance to the action shortcut function or `MutationRef` function.
```typescript
addGameToMyLibrary(dc: DataConnect, vars: AddGameToMyLibraryVariables): MutationPromise<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;

interface AddGameToMyLibraryRef {
  ...
  (dc: DataConnect, vars: AddGameToMyLibraryVariables): MutationRef<AddGameToMyLibraryData, AddGameToMyLibraryVariables>;
}
export const addGameToMyLibraryRef: AddGameToMyLibraryRef;
```

If you need the name of the operation without creating a ref, you can retrieve the operation name by calling the `operationName` property on the addGameToMyLibraryRef:
```typescript
const name = addGameToMyLibraryRef.operationName;
console.log(name);
```

### Variables
The `AddGameToMyLibrary` mutation requires an argument of type `AddGameToMyLibraryVariables`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:

```typescript
export interface AddGameToMyLibraryVariables {
  gameId: UUIDString;
  status: string;
  notes?: string | null;
  completionStatus?: string | null;
}
```
### Return Type
Recall that executing the `AddGameToMyLibrary` mutation returns a `MutationPromise` that resolves to an object with a `data` property.

The `data` property is an object of type `AddGameToMyLibraryData`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:
```typescript
export interface AddGameToMyLibraryData {
  userGame_insert: UserGame_Key;
}
```
### Using `AddGameToMyLibrary`'s action shortcut function

```typescript
import { getDataConnect } from 'firebase/data-connect';
import { connectorConfig, addGameToMyLibrary, AddGameToMyLibraryVariables } from '@dataconnect/generated';

// The `AddGameToMyLibrary` mutation requires an argument of type `AddGameToMyLibraryVariables`:
const addGameToMyLibraryVars: AddGameToMyLibraryVariables = {
  gameId: ..., 
  status: ..., 
  notes: ..., // optional
  completionStatus: ..., // optional
};

// Call the `addGameToMyLibrary()` function to execute the mutation.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await addGameToMyLibrary(addGameToMyLibraryVars);
// Variables can be defined inline as well.
const { data } = await addGameToMyLibrary({ gameId: ..., status: ..., notes: ..., completionStatus: ..., });

// You can also pass in a `DataConnect` instance to the action shortcut function.
const dataConnect = getDataConnect(connectorConfig);
const { data } = await addGameToMyLibrary(dataConnect, addGameToMyLibraryVars);

console.log(data.userGame_insert);

// Or, you can use the `Promise` API.
addGameToMyLibrary(addGameToMyLibraryVars).then((response) => {
  const data = response.data;
  console.log(data.userGame_insert);
});
```

### Using `AddGameToMyLibrary`'s `MutationRef` function

```typescript
import { getDataConnect, executeMutation } from 'firebase/data-connect';
import { connectorConfig, addGameToMyLibraryRef, AddGameToMyLibraryVariables } from '@dataconnect/generated';

// The `AddGameToMyLibrary` mutation requires an argument of type `AddGameToMyLibraryVariables`:
const addGameToMyLibraryVars: AddGameToMyLibraryVariables = {
  gameId: ..., 
  status: ..., 
  notes: ..., // optional
  completionStatus: ..., // optional
};

// Call the `addGameToMyLibraryRef()` function to get a reference to the mutation.
const ref = addGameToMyLibraryRef(addGameToMyLibraryVars);
// Variables can be defined inline as well.
const ref = addGameToMyLibraryRef({ gameId: ..., status: ..., notes: ..., completionStatus: ..., });

// You can also pass in a `DataConnect` instance to the `MutationRef` function.
const dataConnect = getDataConnect(connectorConfig);
const ref = addGameToMyLibraryRef(dataConnect, addGameToMyLibraryVars);

// Call `executeMutation()` on the reference to execute the mutation.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await executeMutation(ref);

console.log(data.userGame_insert);

// Or, you can use the `Promise` API.
executeMutation(ref).then((response) => {
  const data = response.data;
  console.log(data.userGame_insert);
});
```

## UpdateGameStatusInMyLibrary
You can execute the `UpdateGameStatusInMyLibrary` mutation using the following action shortcut function, or by calling `executeMutation()` after calling the following `MutationRef` function, both of which are defined in [dataconnect-generated/index.d.ts](./index.d.ts):
```typescript
updateGameStatusInMyLibrary(vars: UpdateGameStatusInMyLibraryVariables): MutationPromise<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;

interface UpdateGameStatusInMyLibraryRef {
  ...
  /* Allow users to create refs without passing in DataConnect */
  (vars: UpdateGameStatusInMyLibraryVariables): MutationRef<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;
}
export const updateGameStatusInMyLibraryRef: UpdateGameStatusInMyLibraryRef;
```
You can also pass in a `DataConnect` instance to the action shortcut function or `MutationRef` function.
```typescript
updateGameStatusInMyLibrary(dc: DataConnect, vars: UpdateGameStatusInMyLibraryVariables): MutationPromise<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;

interface UpdateGameStatusInMyLibraryRef {
  ...
  (dc: DataConnect, vars: UpdateGameStatusInMyLibraryVariables): MutationRef<UpdateGameStatusInMyLibraryData, UpdateGameStatusInMyLibraryVariables>;
}
export const updateGameStatusInMyLibraryRef: UpdateGameStatusInMyLibraryRef;
```

If you need the name of the operation without creating a ref, you can retrieve the operation name by calling the `operationName` property on the updateGameStatusInMyLibraryRef:
```typescript
const name = updateGameStatusInMyLibraryRef.operationName;
console.log(name);
```

### Variables
The `UpdateGameStatusInMyLibrary` mutation requires an argument of type `UpdateGameStatusInMyLibraryVariables`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:

```typescript
export interface UpdateGameStatusInMyLibraryVariables {
  gameId: UUIDString;
  status: string;
  notes?: string | null;
  completionStatus?: string | null;
}
```
### Return Type
Recall that executing the `UpdateGameStatusInMyLibrary` mutation returns a `MutationPromise` that resolves to an object with a `data` property.

The `data` property is an object of type `UpdateGameStatusInMyLibraryData`, which is defined in [dataconnect-generated/index.d.ts](./index.d.ts). It has the following fields:
```typescript
export interface UpdateGameStatusInMyLibraryData {
  userGame_update?: UserGame_Key | null;
}
```
### Using `UpdateGameStatusInMyLibrary`'s action shortcut function

```typescript
import { getDataConnect } from 'firebase/data-connect';
import { connectorConfig, updateGameStatusInMyLibrary, UpdateGameStatusInMyLibraryVariables } from '@dataconnect/generated';

// The `UpdateGameStatusInMyLibrary` mutation requires an argument of type `UpdateGameStatusInMyLibraryVariables`:
const updateGameStatusInMyLibraryVars: UpdateGameStatusInMyLibraryVariables = {
  gameId: ..., 
  status: ..., 
  notes: ..., // optional
  completionStatus: ..., // optional
};

// Call the `updateGameStatusInMyLibrary()` function to execute the mutation.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await updateGameStatusInMyLibrary(updateGameStatusInMyLibraryVars);
// Variables can be defined inline as well.
const { data } = await updateGameStatusInMyLibrary({ gameId: ..., status: ..., notes: ..., completionStatus: ..., });

// You can also pass in a `DataConnect` instance to the action shortcut function.
const dataConnect = getDataConnect(connectorConfig);
const { data } = await updateGameStatusInMyLibrary(dataConnect, updateGameStatusInMyLibraryVars);

console.log(data.userGame_update);

// Or, you can use the `Promise` API.
updateGameStatusInMyLibrary(updateGameStatusInMyLibraryVars).then((response) => {
  const data = response.data;
  console.log(data.userGame_update);
});
```

### Using `UpdateGameStatusInMyLibrary`'s `MutationRef` function

```typescript
import { getDataConnect, executeMutation } from 'firebase/data-connect';
import { connectorConfig, updateGameStatusInMyLibraryRef, UpdateGameStatusInMyLibraryVariables } from '@dataconnect/generated';

// The `UpdateGameStatusInMyLibrary` mutation requires an argument of type `UpdateGameStatusInMyLibraryVariables`:
const updateGameStatusInMyLibraryVars: UpdateGameStatusInMyLibraryVariables = {
  gameId: ..., 
  status: ..., 
  notes: ..., // optional
  completionStatus: ..., // optional
};

// Call the `updateGameStatusInMyLibraryRef()` function to get a reference to the mutation.
const ref = updateGameStatusInMyLibraryRef(updateGameStatusInMyLibraryVars);
// Variables can be defined inline as well.
const ref = updateGameStatusInMyLibraryRef({ gameId: ..., status: ..., notes: ..., completionStatus: ..., });

// You can also pass in a `DataConnect` instance to the `MutationRef` function.
const dataConnect = getDataConnect(connectorConfig);
const ref = updateGameStatusInMyLibraryRef(dataConnect, updateGameStatusInMyLibraryVars);

// Call `executeMutation()` on the reference to execute the mutation.
// You can use the `await` keyword to wait for the promise to resolve.
const { data } = await executeMutation(ref);

console.log(data.userGame_update);

// Or, you can use the `Promise` API.
executeMutation(ref).then((response) => {
  const data = response.data;
  console.log(data.userGame_update);
});
```

