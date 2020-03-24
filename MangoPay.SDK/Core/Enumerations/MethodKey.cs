﻿
namespace MangoPay.SDK.Core.Enumerations
{
    /// <summary>Method key enumeration.</summary>
    public enum MethodKey
    {
        AuthenticationBase,
        AuthenticationOAuth,
        ApplePayinsDirectCreate,
        CardGet,
        CardSave,
        CardRegistrationCreate,
        CardRegistrationGet,
        CardRegistrationSave,
        CardPreauthorizations,
        CardTransactions,
        CardByFingerprintGet,
        EventsAll,
        GooglePayinsDirectCreate,
        HooksAll,
        HooksCreate,
        HooksGet,
        HooksSave,
        PayinsPayPalCreate,
        PayinsBankwireDirectCreate,
        PayinsCardDirectCreate,
        PayinsCardWebCreate,
        PayinsCardWebGetCardData,
        PayinsCreateRefunds,
        PayinsGetRefunds,
        PayinsGet,
        PayinsPreauthorizedDirectCreate,
        PayinsDirectDebitCreate,
        PayinsMandateDirectDebitCreate,
        PayoutsBankwireCreate,
        PayoutsGet,
        PayoutsGetRefunds,
        PreauthorizationCreate,
        PreauthorizationGet,
        PreauthorizationSave,
        RefundsGet,
        TransfersCreate,
        TransfersCreateRefunds,
        TransfersGet,
        TransfersGetRefunds,
        UsersAll,
        UsersAllBankAccount,
        UsersAllCards,
        UsersAllTransactions,
        UsersAllWallets,
        UsersPreauthorizations,
        UsersCreateBankAccountsCa,
        UsersCreateBankAccountsGb,
        UsersCreateBankAccountsIban,
        UsersCreateBankAccountsOther,
        UsersCreateBankAccountsUs,
        UsersCreateKycDocument,
        UsersCreateKycPage,
        UsersCreateLegals,
        UsersCreateNaturals,
        UsersGet,
        UsersGetBankAccount,
        UsersGetKycDocument,
        UsersGetKycDocuments,
        UsersGetLegals,
        UsersGetNaturals,
        UsersSaveKycDocument,
        UsersSaveLegals,
        UsersSaveNaturals,
        UsersSaveBankAccount,
        UsersEmoneyGet,
        UsersEmoneyYearGet,
        UsersEmoneyMonthGet,

        WalletsAllTransactions,
        WalletsCreate,
        WalletsGet,
        WalletsSave,
        BankingAliasCreateIban,
        BankingAliasAll,
        BankingAliasGet,
        BankingAliasSave,

        ClientGetKycDocuments,
        GetKycDocument,
		KycDocumentConsult,

		ClientGetWalletsDefault,
        ClientGetWalletsFees,
        ClientGetWalletsCredit,
        ClientGetWalletsDefaultWithCurrency,
        ClientGetWalletsFeesWithCurrency,
        ClientGetWalletsCreditWithCurrency,
        ClientGetTransactions,
        ClientGetWalletTransactions,
        ClientCreateBankwireDirect,
        ClientGet,
        ClientSave,
        ClientUploadLogo,
        
        DisputesGet,
        DisputesSaveTag,
        DisputesSaveContestFunds,
        DisputeSaveClose,
        DisputesGetTransactions,        
        DisputesGetAll,
        DisputesGetForWallet,
        DisputesGetForUser,
        DisputesGetPendingSettlement,
        DisputesDocumentCreate,
        DisputesDocumentPageCreate,
        DisputesDocumentSubmit,
        DisputesDocumentGet,
        DisputesDocumentGetForDispute,
        DisputesDocumentGetForClient,
        DisputesRepudiationGet,
        DisputesRepudiationGetRefunds,
        DisputesRepudiationCreateSettlement,
		DisputesDocumentConsult,
		SettlementsGet,

        IdempotencyResponseGet,

        MandateCreate,
        MandateGet,
        MandateCancel,
        MandatesGetAll,
		MandatesGetTransactions,
        MandatesGetForUser,
        MandatesGetForBankAccount,

        ReportRequest,
        ReportGetAll,
        ReportGet,

        SingleSignOnAll,
        SingleSignOnCreate,
        SingleSignOnGet,
        SingleSignOnSave,
        SingleSignOnExtendInvitation,

        PermissionGroupAll,
        PermissionGroupAllSsos,
        PermissionGroupCreate,
        PermissionGroupGet,
        PermissionGroupSave,

		SingleSignOnsMe,
		SingleSignOnsMePermissionGroup,

		UboDeclarationCreate,
		UboDeclarationUpdate,
		UboDeclarationsGet,
		UboDeclarationGet,
		UboGet,
		UboCreate,
		UboUpdate,

		BankAccountsGetTransactions
	}
}
