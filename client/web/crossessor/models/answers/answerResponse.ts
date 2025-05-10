import AnswerType from "@/enums/answerType"
import ModelType from "@/enums/modelType"
import { UUID } from "crypto"

export type AnswerResponse = {

    id: UUID,
    text: string,
    modelType: ModelType,
    answerType: AnswerType
}