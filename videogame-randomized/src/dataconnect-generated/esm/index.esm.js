import { queryRef, executeQuery, mutationRef, executeMutation, validateArgs } from 'firebase/data-connect';

export const connectorConfig = {
  connector: 'example',
  service: 'videogame-randomized-complete',
  location: 'europe-west1'
};

export const listAllGamesRef = (dc) => {
  const { dc: dcInstance} = validateArgs(connectorConfig, dc, undefined);
  dcInstance._useGeneratedSdk();
  return queryRef(dcInstance, 'ListAllGames');
}
listAllGamesRef.operationName = 'ListAllGames';

export function listAllGames(dc) {
  return executeQuery(listAllGamesRef(dc));
}

export const getMyGamesRef = (dc) => {
  const { dc: dcInstance} = validateArgs(connectorConfig, dc, undefined);
  dcInstance._useGeneratedSdk();
  return queryRef(dcInstance, 'GetMyGames');
}
getMyGamesRef.operationName = 'GetMyGames';

export function getMyGames(dc) {
  return executeQuery(getMyGamesRef(dc));
}

export const addGameToMyLibraryRef = (dcOrVars, vars) => {
  const { dc: dcInstance, vars: inputVars} = validateArgs(connectorConfig, dcOrVars, vars, true);
  dcInstance._useGeneratedSdk();
  return mutationRef(dcInstance, 'AddGameToMyLibrary', inputVars);
}
addGameToMyLibraryRef.operationName = 'AddGameToMyLibrary';

export function addGameToMyLibrary(dcOrVars, vars) {
  return executeMutation(addGameToMyLibraryRef(dcOrVars, vars));
}

export const updateGameStatusInMyLibraryRef = (dcOrVars, vars) => {
  const { dc: dcInstance, vars: inputVars} = validateArgs(connectorConfig, dcOrVars, vars, true);
  dcInstance._useGeneratedSdk();
  return mutationRef(dcInstance, 'UpdateGameStatusInMyLibrary', inputVars);
}
updateGameStatusInMyLibraryRef.operationName = 'UpdateGameStatusInMyLibrary';

export function updateGameStatusInMyLibrary(dcOrVars, vars) {
  return executeMutation(updateGameStatusInMyLibraryRef(dcOrVars, vars));
}

