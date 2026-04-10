/**
 * JSDoc mirrors of backend DTOs (ASP.NET serializes PascalCase properties as camelCase JSON).
 * @see videogame-randomized-back/DTOs/
 */

/**
 * @typedef {Object} GenreDto
 * @property {number} id
 * @property {string} name
 * @property {string} [slug]
 */

/**
 * @typedef {Object} PlatformDto
 * @property {number} id
 * @property {string} name
 * @property {string} [slug]
 */

/**
 * @typedef {Object} GameDto
 * @property {number} id
 * @property {string} name
 * @property {string} [backgroundImage]
 * @property {number} rating
 * @property {string} [released]
 * @property {GenreDto[]} [genres]
 * @property {PlatformDto[]} [platforms]
 * @property {number} [metacritic]
 * @property {string} [descriptionRaw]
 * @property {number} [personalRating]
 * @property {string} [note]
 * @property {string} savedAt
 * @property {string} userId
 */

/**
 * @typedef {Object} GameStatsDto
 * @property {number} totalGames
 * @property {number} averageRating
 * @property {Record<string, number>} genreCount
 * @property {Record<string, number>} platformCount
 */

/**
 * @typedef {Object} UpdateGameDto
 * @property {number} [personalRating]
 * @property {string} [note]
 */

/**
 * @typedef {Object} AuthResponseDto
 * @property {string} token
 * @property {string} email
 */

/**
 * RAWG API game shape used when mapping to CreateGameDto (nested platform).
 * @typedef {Object} RawgGameLike
 * @property {number} id
 * @property {string} name
 * @property {string} [background_image]
 * @property {number} rating
 * @property {string} [released]
 * @property {{ id: number, name: string, slug?: string }[]} [genres]
 * @property {{ platform: { id: number, name: string, slug?: string } }[]} [platforms]
 * @property {number} [metacritic]
 * @property {string} [description_raw]
 */

export {}
