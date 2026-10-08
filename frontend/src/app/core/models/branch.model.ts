export interface Branch {
  branchId: number;
  branchName: string;
  address: string | null;
  /** 'HH:mm:ss' */
  openingTime: string;
  /** 'HH:mm:ss' */
  closingTime: string;
}

export interface Studio {
  studioId: number;
  studioName: string;
  branchId: number;
  capacity: number;
}
