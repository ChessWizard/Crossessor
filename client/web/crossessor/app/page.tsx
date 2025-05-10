"use client";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader } from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
} from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";
import ModelType from "@/enums/modelType";
import SystemBehaviourType from "@/enums/systemBehaviourType";
import {
  chatRequestSchema,
  ChatRequest,
} from "@/lib/schemas/chatRequestSchema";
import { zodResolver } from "@hookform/resolvers/zod";
import { DialogTitle } from "@radix-ui/react-dialog";
import { useEffect, useState, useTransition } from "react";
import { useForm } from "react-hook-form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import SystemBehaviourTypeLabels from "@/models/records/systemBehaviourTypeLabels";
import api from "@/lib/api/baseApiCall";
import { Loader, ArrowLeft } from "lucide-react";
import { Result } from "@/models/result/result";
import { AnswerResponse } from "@/models/answers/answerResponse";
import ModelTypeLabels from "@/models/records/modelTypeLabels";
import Markdown from "@/components/customized/markdown";
import {
  HelpCircle,
  MessageSquare,
  BarChart2,
  RotateCcw,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { EvaluationResponse } from "@/models/evaluations/evaluationResponse";
import { CrossCompareRequest } from "@/lib/schemas/crossCompareRequestSchema";
import { UUID } from "crypto";

enum CompareStatus {
  Setup,
  Evaluated,
}

export default function Home() {
  const [questionModalIsOpen, setQuestionModelIsOpen] = useState(true);
  const [isStartChatButtonDisabled, setIsStartChatButtonDisabled] =
    useState(true);
  const [isQuestionFormPending, startQuestionFormTransition] = useTransition();
  const [compareStatus, setCompareStatus] = useState<CompareStatus>(
    CompareStatus.Setup
  );
  const [chatResult, setChatResult] = useState<Result<AnswerResponse[]>>();
  const [selectedModel, setSelectedModel] = useState<ModelType | null>(null);
  const [isCrossComparePending, startCrossCompareTransition] = useTransition()
  const [crossCompareResult, setCrossCompareResult] = useState<Result<EvaluationResponse[]>>()

  const form = useForm<ChatRequest>({
    resolver: zodResolver(chatRequestSchema),
    defaultValues: {
      question: "",
      systemBehaviourType: SystemBehaviourType.MathProfessor,
      targetModelTypes: [ModelType.GPT_4o, ModelType.Gemini_1_5_Flash],
    },
  });

  const question = form.watch("question");
  const systemBehaviourType = form.watch("systemBehaviourType");

  const startChatAsync = async (data: ChatRequest) => {

    setCrossCompareResult(undefined)

    startQuestionFormTransition(async () => {
      try {
        const response = await api.post<Result<AnswerResponse[]>>(
          "/Chats/Multiple",
          data
        );
        setChatResult(response.data);
        setCompareStatus(CompareStatus.Evaluated);
      } catch (error) {
        console.log("Chat Fetch Error: ", error);
      }
    });
  };

  const crossCompareAsync = async () => {
    startCrossCompareTransition(async () => {
      try {
        const data = {
          systemBehaviourType: systemBehaviourType,
          answerIds: chatResult?.data.map(result => result.id) as UUID[]
        } as CrossCompareRequest;
        
        const response = await api.post<Result<EvaluationResponse[]>>(
          "/Compare/Cross",
          data
        );

        setCrossCompareResult(response.data);
      } catch (error) {
        console.log("Cross Compare Error: ", error);
      }
    });
  };

  useEffect(() => {
    const isStartCompareButtonStatusDisabled =
      !question || question.length === 0 || !systemBehaviourType;

    setIsStartChatButtonDisabled(isStartCompareButtonStatusDisabled);
  }, [question, systemBehaviourType]);

  useEffect(() => {
    if (chatResult && chatResult.data.length > 0 && !selectedModel) {
      setSelectedModel(chatResult.data[0].modelType);
    }
  }, [chatResult, selectedModel]);

  const systemBehaviourTypes = Object.values(SystemBehaviourType).filter(
    (v) => typeof v === "number"
  ) as SystemBehaviourType[];

  const modelTypes = Object.values(ModelType).filter(
    (v) => typeof v === "number"
  ) as ModelType[];

  return (
    <>
      {isQuestionFormPending && (
        <div className="fixed inset-0 flex items-center justify-center bg-background/80 backdrop-blur-sm">
          <div className="flex flex-col items-center gap-2">
            <Loader className="h-8 w-8 animate-spin" />
            <p className="text-sm text-muted-foreground">
              Yapay Zekalar Değerlendiriyor...
            </p>
          </div>
        </div>
      )}

      {compareStatus === CompareStatus.Setup && !isQuestionFormPending && (
        <Dialog
          open={questionModalIsOpen}
          onOpenChange={setQuestionModelIsOpen}
        >
          <DialogContent
            className="w-full p-3 md:min-w-[600px] lg:min-w-[700px] max-h-[90vh] overflow-y-auto [&>button]:hidden"
            onPointerDownOutside={(e) => e.preventDefault()}
            onEscapeKeyDown={(e) => e.preventDefault()}
          >
            <DialogHeader className="flex flex-row items-center justify-between">
              {chatResult && chatResult.data && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => setCompareStatus(CompareStatus.Evaluated)}
                >
                  <ArrowLeft className="h-4 w-4" />
                </Button>
              )}

              <DialogTitle className="text-center flex-1">
                Yapay Zekalara Sor
              </DialogTitle>
              <div className="w-8" />
            </DialogHeader>
            <Form {...form}>
              <form
                onSubmit={form.handleSubmit(async (data) => {
                  await startChatAsync({
                    ...data,
                    targetModelTypes: data.targetModelTypes.map(Number),
                  });
                })}
              >
                <FormField
                  control={form.control}
                  name="systemBehaviourType"
                  render={({ field }) => (
                    <FormItem className="mb-5">
                      <FormLabel>Sistem Davranışı</FormLabel>
                      <FormControl>
                        <>
                          <div
                            className={`hidden md:grid md:grid-cols-3 gap-2`}
                          >
                            {systemBehaviourTypes.map((type) => (
                              <Button
                                key={type}
                                type="button"
                                variant={
                                  field.value === type ? "default" : "outline"
                                }
                                onClick={() => field.onChange(type)}
                              >
                                {SystemBehaviourTypeLabels[type]}
                              </Button>
                            ))}
                          </div>
                          <div className="md:hidden">
                            <Select
                              value={field.value ? String(field.value) : ""}
                              onValueChange={(val: any) =>
                                field.onChange(Number(val))
                              }
                            >
                              <SelectTrigger className="w-full">
                                <SelectValue placeholder="Sistem Davranışı Seçin" />
                              </SelectTrigger>
                              <SelectContent>
                                {systemBehaviourTypes.map((type) => (
                                  <SelectItem
                                    key={type}
                                    value={String(type)}
                                    className="text-center"
                                  >
                                    {SystemBehaviourTypeLabels[type]}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                        </>
                      </FormControl>
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="question"
                  render={({ field }) => (
                    <FormItem className="mb-4">
                      <FormLabel>Soru</FormLabel>
                      <FormDescription className="text-xs text-gray-600 md:text-sm">
                        Sormak istediğiniz soruyu gayet açıklayıcı bir biçimde
                        anlatarak daha iyi sonuçlar alabilirsiniz.
                      </FormDescription>
                      <FormControl>
                        <Textarea placeholder="Sorunuz..." {...field} />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <Button
                  className="w-full
          font-bold 
          disabled:bg-black
          disabled:opacity-50
          disabled:cursor-not-allowed"
                  type="submit"
                  disabled={isStartChatButtonDisabled}
                >
                  {isQuestionFormPending ? (
                    <Loader className="w-4 h-4 animate-spin" />
                  ) : (
                    "SOR"
                  )}
                </Button>
              </form>
            </Form>
          </DialogContent>
        </Dialog>
      )}

      {compareStatus === CompareStatus.Evaluated && (
        <>
          <div className="hidden md:min-h-screen md:flex md:items-center">
            <div className="hidden md:grid md:grid-cols-2 gap-4 p-6 w-full max-w-7xl mx-auto">
              {chatResult?.data.map((result: AnswerResponse, index) => {
                const evaluation = crossCompareResult?.data?.find(
                  evaluate => evaluate.answer.modelType !== result.modelType
                );

                return (
                  <div key={index} className="p-4 rounded-lg border">
                    <div className="flex items-center gap-2 mb-4">
                      <h3 className="font-semibold">
                        {ModelTypeLabels[result.modelType]}
                      </h3>
                    </div>
                    <div className="prose prose-sm max-w-none mb-6">
                      <Markdown content={result.text} />
                    </div>
                    
                    {isCrossComparePending ? (
                      <div className="mt-4 pt-4 border-t">
                        <div className="flex items-center justify-center gap-2">
                          <Loader className="h-4 w-4 animate-spin" />
                          <span className="text-sm text-muted-foreground">Değerlendiriliyor...</span>
                        </div>
                      </div>
                    ) : evaluation ? (
                      <div className="mt-4 pt-4 border-t">
                        <h3 className="text-base font-semibold mb-3">Değerlendirme Sonuçları</h3>
                        <h4 className="text-sm font-semibold mb-3">
                          Değerlendiren: {ModelTypeLabels[evaluation.answer.modelType as ModelType]}
                        </h4>
                        <div className="grid grid-cols-2 gap-2 text-sm">
                          <div className="flex justify-between">
                            <span>Doğruluk:</span>
                            <span>{evaluation.accuracy}%</span>
                          </div>
                          <div className="flex justify-between">
                            <span>Eksiksizlik:</span>
                            <span>{evaluation.completeness}%</span>
                          </div>
                          <div className="flex justify-between">
                            <span>Netlik:</span>
                            <span>{evaluation.clarity}%</span>
                          </div>
                          <div className="flex justify-between">
                            <span>Tarafsızlık:</span>
                            <span>{evaluation.neutrality}%</span>
                          </div>
                          <div className="col-span-2 flex justify-between font-semibold mt-2 pt-2 border-t">
                            <span>Genel Puan:</span>
                            <span>{evaluation.overallScore.toFixed(2)}%</span>
                          </div>
                        </div>
                      </div>
                    ) : null}
                  </div>
                );
              })}
            </div>
          </div>

          <div className="md:hidden p-4 pb-24">
            <div className="flex justify-center gap-2 mb-4 overflow-x-auto">
              {modelTypes.map((type, index) => (
                <Button
                  key={index}
                  variant={selectedModel === type ? "default" : "outline"}
                  onClick={() => setSelectedModel(type)}
                  className="whitespace-nowrap"
                >
                  {ModelTypeLabels[type]}
                </Button>
              ))}
            </div>

            <div className="p-4 rounded-lg border">
              {chatResult?.data.find((r) => r.modelType === selectedModel) && (
                <>
                  <div className="prose prose-sm max-w-none mb-6">
                    <Markdown
                      content={
                        chatResult?.data.find((r) => r.modelType === selectedModel)
                          ?.text || ""
                      }
                    />
                  </div>
                  
                  {isCrossComparePending ? (
                    <div className="mt-4 pt-4 border-t">
                      <div className="flex items-center justify-center gap-2">
                        <Loader className="h-4 w-4 animate-spin" />
                        <span className="text-sm text-muted-foreground">Değerlendiriliyor...</span>
                      </div>
                    </div>
                  ) : crossCompareResult?.data.find(
                    evaluation => evaluation.answer.modelType !== selectedModel
                  ) ? (
                    <div className="mt-4 pt-4 border-t">
                      <h3 className="text-base font-semibold mb-3">Değerlendirme Sonuçları</h3>
                      <h4 className="text-sm font-semibold mb-3">
                        Değerlendiren: {ModelTypeLabels[crossCompareResult?.data.find(
                          evaluation => evaluation.answer.modelType !== selectedModel
                        )?.answer.modelType as ModelType]}
                      </h4>
                      <div className="grid grid-cols-2 gap-2 text-sm">
                        <div className="flex justify-between">
                          <span>Doğruluk:</span>
                          <span>{crossCompareResult.data.find(evaluation => evaluation.answer.modelType !== selectedModel)?.accuracy}%</span>
                        </div>
                        <div className="flex justify-between">
                          <span>Eksiksizlik:</span>
                          <span>{crossCompareResult.data.find(evaluation => evaluation.answer.modelType !== selectedModel)?.completeness}%</span>
                        </div>
                        <div className="flex justify-between">
                          <span>Netlik:</span>
                          <span>{crossCompareResult.data.find(evaluation => evaluation.answer.modelType !== selectedModel)?.clarity}%</span>
                        </div>
                        <div className="flex justify-between">
                          <span>Tarafsızlık:</span>
                          <span>{crossCompareResult.data.find(evaluation => evaluation.answer.modelType !== selectedModel)?.neutrality}%</span>
                        </div>
                        <div className="col-span-2 flex justify-between font-semibold mt-2 pt-2 border-t">
                          <span>Genel Puan:</span>
                          <span>{crossCompareResult.data.find(evaluation => evaluation.answer.modelType !== selectedModel)?.overallScore.toFixed(2)}%</span>
                        </div>
                      </div>
                    </div>
                  ) : null}
                </>
              )}
            </div>
          </div>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                size="icon"
                variant="outline"
                className="fixed bottom-6 right-6 rounded-full w-12 h-12 shadow-lg hover:shadow-xl transition-shadow"
              >
                <HelpCircle className="h-6 w-6" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end">
              <DropdownMenuItem
                onClick={crossCompareAsync}
              >
                <BarChart2 className="mr-2 h-4 w-4" />
                <span>Çapraz Karşılaştır</span>
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setCompareStatus(CompareStatus.Setup)}
              >
                <MessageSquare className="mr-2 h-4 w-4" />
                <span>Yeni Soru Sor</span>
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => {
                  const currentFormData = form.getValues()
                  startChatAsync({
                    ...currentFormData,
                    targetModelTypes: currentFormData.targetModelTypes.map(Number)
                  })

                }}
              >
                <RotateCcw className="mr-2 h-4 w-4" />
                <span>Yeniden Cevapla</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </>
      )}
    </>
  );
}
