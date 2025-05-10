import SystemBehaviourType from "@/enums/systemBehaviourType";
import { z } from "zod";

export const crossCompareRequestSchema =  z.object({
    systemBehaviourType: z.nativeEnum(SystemBehaviourType),
    answerIds: z.array(z.string().uuid())
})

export type CrossCompareRequest = z.infer<typeof crossCompareRequestSchema>