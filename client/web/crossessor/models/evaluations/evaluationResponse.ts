import { UUID } from "crypto"
import { AnswerResponse } from "../answers/answerResponse"

export type EvaluationResponse = {
    id: UUID,
    accuracy: number,
    completeness: number,
    clarity: number,
    neutrality: number,
    overallScore: number,
    answer: AnswerResponse
}