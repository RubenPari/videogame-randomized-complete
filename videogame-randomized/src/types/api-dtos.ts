/**
 * TypeScript mirrors of backend DTOs (ASP.NET serializes PascalCase properties as camelCase JSON).
 * @see videogame-randomized-back/DTOs/
 */

export interface GenreDto {
  id: number
  name: string
  slug?: string
}

export interface PlatformDto {
  id: number
  name: string
  slug?: string
}

export interface GameDto {
  id: number
  name: string
  backgroundImage?: string
  rating: number
  released?: string
  genres?: GenreDto[]
  platforms?: PlatformDto[]
  metacritic?: number
  descriptionRaw?: string
  personalRating?: number
  note?: string
  savedAt: string
  userId: string
}

export interface GameStatsDto {
  totalGames: number
  averageRating: number
  genreCount: Record<string, number>
  platformCount: Record<string, number>
}

export interface UpdateGameDto {
  personalRating?: number
  note?: string
}

export interface AuthResponseDto {
  token: string
  email: string
}

export interface RawgGenreLike {
  id: number
  name: string
  slug?: string
}

export interface RawgPlatformLike {
  platform: {
    id: number
    name: string
    slug?: string
  }
}

/**
 * RAWG API game shape used when mapping to CreateGameDto (nested platform).
 */
export interface RawgGameLike {
  id: number
  name: string
  background_image?: string
  backgroundImage?: string
  rating: number
  released?: string
  genres?: RawgGenreLike[]
  platforms?: RawgPlatformLike[]
  metacritic?: number
  description_raw?: string
}

/**
 * Combined game type that supports both backend DTO (camelCase) and RAWG API (snake_case, nested platforms).
 */
export interface GameShape {
  id: number
  name: string
  backgroundImage?: string
  background_image?: string
  rating: number
  released?: string
  genres?: (GenreDto | RawgGenreLike)[]
  platforms?: (PlatformDto | RawgPlatformLike)[]
  metacritic?: number
  descriptionRaw?: string
  description_raw?: string
  personalRating?: number
  note?: string
  savedAt?: string
  userId?: string
}

/**
 * Combined game type that supports both backend DTO (camelCase) and RAWG API (snake_case, nested platforms).
 */
export interface GameShape {
  id: number
  name: string
  backgroundImage?: string
  background_image?: string
  rating: number
  released?: string
  genres?: (GenreDto | RawgGenreLike)[]
  platforms?: (PlatformDto | RawgPlatformLike)[]
  metacritic?: number
  descriptionRaw?: string
  description_raw?: string
  personalRating?: number
  note?: string
  savedAt?: string
  userId?: string
}
