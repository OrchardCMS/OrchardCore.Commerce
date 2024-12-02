import type { IText } from "@/common";

export interface ITotal {
    Amount: string;
    Currency: string;
}

export interface IPaymentIntentSecret {
    ClientSecret: string;
}

export interface IPaymentDetails {
    company: IText;
    firstName: IText;
    lastName: IText;
    street: IText;
    zipCode: IText;
    city: IText;
    country: IText;
}
