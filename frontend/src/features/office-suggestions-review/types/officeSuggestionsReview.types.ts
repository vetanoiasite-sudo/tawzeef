/** Mirrors backend/src/Tawzeef.Application/Features/Suggestions/SuggestionDtos.cs. */

export type SuggestionStatusCode = 'New' | 'UnderReview' | 'Responded' | 'Closed'

export type Suggestion = {
  id: string
  content: string
  status: SuggestionStatusCode
  officeNotes: string | null
  creationDate: string
}

export type SuggestionQueueFilter = {
  status?: SuggestionStatusCode
  pageIndex: number
  pageSize: number
}
