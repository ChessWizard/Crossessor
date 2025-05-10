export interface ErrorResult {
    errors: string[];
    isShow: boolean;
  }
  
  export interface BaseResult<TData> {
    data: TData;
    message: string;
    httpStatusCode: number;
    errorDto: ErrorResult | null;
  }
  
  export type Result<TData> = BaseResult<TData>
  
  export interface PagingMetaData {
    pageSize: number;
    currentPage: number;
    totalPages: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
  }
  
  export interface PagingResult<TData> extends BaseResult<TData> {
    pagingMetaData: PagingMetaData | null;
  }
  