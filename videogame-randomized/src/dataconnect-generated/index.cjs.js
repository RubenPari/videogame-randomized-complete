const { queryRef, executeQuery, mutationRef, executeMutation, validateArgs } = require('firebase/data-connect');

const connectorConfig = {
  connector: 'example',
  service: 'videogame-randomized-complete',
  location: 'europe-west1'
};
exports.connectorConfig = connectorConfig;

const listAllGamesRef = (dc) => {
  const { dc: dcInstance} = validateArgs(connectorConfig, dc, undefined);
  dcInstance._useGeneratedSdk();
  return queryRef(dcInstance, 'ListAllGames');
}
listAllGamesRef.operationName = 'ListAllGames';
exports.listAllGamesRef = listAllGamesRef;

exports.listAllGames = function listAllGames(dc) {
  return executeQuery(listAllGamesRef(dc));
};

const getMyGamesRef = (dc) => {
  const { dc: dcInstance} = validateArgs(connectorConfig, dc, undefined);
  dcInstance._useGeneratedSdk();
  return queryRef(dcInstance, 'GetMyGames');
}
getMyGamesRef.operationName = 'GetMyGames';
exports.getMyGamesRef = getMyGamesRef;

exports.getMyGames = function getMyGames(dc) {
  return executeQuery(getMyGamesRef(dc));
};

const addGameToMyLibraryRef = (dcOrVars, vars) => {
  const { dc: dcInstance, vars: inputVars} = validateArgs(connectorConfig, dcOrVars, vars, true);
  dcInstance._useGeneratedSdk();
  return mutationRef(dcInstance, 'AddGameToMyLibrary', inputVars);
}
addGameToMyLibraryRef.operationName = 'AddGameToMyLibrary';
exports.addGameToMyLibraryRef = addGameToMyLibraryRef;

exports.addGameToMyLibrary = function addGameToMyLibrary(dcOrVars, vars) {
  return executeMutation(addGameToMyLibraryRef(dcOrVars, vars));
};

const updateGameStatusInMyLibraryRef = (dcOrVars, vars) => {
  const { dc: dcInstance, vars: inputVars} = validateArgs(connectorConfig, dcOrVars, vars, true);
  dcInstance._useGeneratedSdk();
  return mutationRef(dcInstance, 'UpdateGameStatusInMyLibrary', inputVars);
}
updateGameStatusInMyLibraryRef.operationName = 'UpdateGameStatusInMyLibrary';
exports.updateGameStatusInMyLibraryRef = updateGameStatusInMyLibraryRef;

exports.updateGameStatusInMyLibrary = function updateGameStatusInMyLibrary(dcOrVars, vars) {
  return executeMutation(updateGameStatusInMyLibraryRef(dcOrVars, vars));
};
