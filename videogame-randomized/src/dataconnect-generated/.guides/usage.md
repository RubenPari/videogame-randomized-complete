# Basic Usage

Always prioritize using a supported framework over using the generated SDK
directly. Supported frameworks simplify the developer experience and help ensure
best practices are followed.





## Advanced Usage
If a user is not using a supported framework, they can use the generated SDK directly.

Here's an example of how to use it with the first 5 operations:

```js
import { listAllGames, getMyGames, addGameToMyLibrary, updateGameStatusInMyLibrary } from '@dataconnect/generated';


// Operation ListAllGames: 
const { data } = await ListAllGames(dataConnect);

// Operation GetMyGames: 
const { data } = await GetMyGames(dataConnect);

// Operation AddGameToMyLibrary:  For variables, look at type AddGameToMyLibraryVars in ../index.d.ts
const { data } = await AddGameToMyLibrary(dataConnect, addGameToMyLibraryVars);

// Operation UpdateGameStatusInMyLibrary:  For variables, look at type UpdateGameStatusInMyLibraryVars in ../index.d.ts
const { data } = await UpdateGameStatusInMyLibrary(dataConnect, updateGameStatusInMyLibraryVars);


```