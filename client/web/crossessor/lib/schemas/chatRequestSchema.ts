import ModelType from "@/enums/modelType";
import SystemBehaviourType from "@/enums/systemBehaviourType";
import { z } from "zod";

export const chatRequestSchema =  z.object({
    question: z.string(),
    systemBehaviourType: z.nativeEnum(SystemBehaviourType),
    targetModelTypes: z.array(z.nativeEnum(ModelType))
})

export type ChatRequest = z.infer<typeof chatRequestSchema>
