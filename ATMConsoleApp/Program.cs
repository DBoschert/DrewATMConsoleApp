

/*
$ Enter Card Code : 1234<enter>
$ Enter Pin Code  : 6789<enter>
Then there should be a menu showing the customer what the options are for the ATM. Options should include:

* Balance
* Deposit
* Withdraw
* Transfer
* Show Transactions
*/

using ATMConsoleApp.Models;
using ATMConsoleApp;
using System.Text.Json;

const string baseurl = "http://localhost:1111";

HttpClient http = new HttpClient();

JsonSerializerOptions joptions = new JsonSerializerOptions() {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

Customer? customer;
//Account? account;

// RUN ATM LOGIC
while (true) {
    // LOGIN PROMPT
    Console.Write("Enter Card Code: ");
    int cardCode = Convert.ToInt32(Console.ReadLine());
    Console.Write("Enter Pin Code: ");
    int pinCode = Convert.ToInt32(Console.ReadLine());
    var jsonResponse = await CustomerLoginAsync(http, joptions, cardCode, pinCode);
    customer = jsonResponse.DataReturned as Customer;
    var status = jsonResponse.HttpStatusCode;
    if (customer == null || status == 404) {
        Console.Clear();
        Console.WriteLine("[Login Failed!]");
        continue;
    }
    
    // MAIN MENU
    var action = GetMenuSelection(customer).ToLower();
    
    // CHECK BALANCE
    if (action == "b") {
        // GetBalance method
        Account account = await SelectAccount(http, joptions, customer.Id);
        Console.WriteLine($"You selected {account.Id} {account.Description}");
    }
    // MAKE DEPOSIT
    else if (action == "d") {
        // MakeDeposit method
    }
    // MAKE WITHDRAW
    else if (action == "w") {
        // MakeWithdraw method
    }
    // MAKE TRANSFER
    else if (action == "t") {
        // MakeTransfer method
    }
    // SHOW TRANSACTIONS
    else if (action == "st") {
        // ShowTransactions method
    }

    // End Program
    break;
}

// (1) CUSTOMER LOGIN
async Task<JsonResponse> CustomerLoginAsync(HttpClient http, JsonSerializerOptions joptions, int cardCode, int pinCode) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/customers/{cardCode}/{pinCode}");
    HttpResponseMessage res = await http.SendAsync(req);
    if (res.StatusCode != System.Net.HttpStatusCode.OK) {
        Console.WriteLine($"Http ErrorCode: {res.StatusCode}");
    }
    var json = await res.Content.ReadAsStringAsync();
    var customer = (Customer?)JsonSerializer.Deserialize(json, typeof(Customer), joptions);
    if (customer is null) throw new Exception();
    return new JsonResponse {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = customer
    };
}

// (2) MAIN MENU PROMPT
string GetMenuSelection(Customer customer) {
    string? selection;
    Console.Clear();
    Console.WriteLine($"Hello {customer!.Name}!\n");
    while (true) {
        Console.WriteLine("'B' = Balance");
        Console.WriteLine("'D' = Deposit");
        Console.WriteLine("'W' = Withdraw");
        Console.WriteLine("'T' = Transfer");
        Console.WriteLine("'ST' = Show Transactions");
        Console.Write("\nPlease make a selection: ");
        selection = Console.ReadLine();
        if (selection is null || selection == "") {
            Console.Clear();
            Console.WriteLine("Please make a valid selection\n");
            continue;
        }
        break;
    }
        return selection!;
}

// (3) ACCOUNT SELECTION MENU
async Task<Account> SelectAccount(HttpClient http, JsonSerializerOptions joptions, int custId) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts");
    HttpResponseMessage res = await http.SendAsync(req);
    if (res.StatusCode != System.Net.HttpStatusCode.OK) {
        Console.WriteLine($"Http ErrorCode: {res.StatusCode}");
    }
    var json = await res.Content.ReadAsStringAsync();
    var accounts = JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), joptions) as IEnumerable<Account>;
    // SELECTION MENU
    Console.Clear();
    Console.WriteLine("AVAILABLE ACCOUNTS\n" +
                      "------------------");
    var custAccounts = accounts.Where(x => x.CustomerId == custId);
    foreach(Account acct in custAccounts) {
        Console.WriteLine($"'{acct.Id}' {acct.Description}");
    }
    Console.Write("\nPlease make a selection: ");
    var acctId = Convert.ToInt32(Console.ReadLine());
    var account = accounts.Where(x => x.Id == acctId).SingleOrDefault();
    return account!;
}



// CHECK BALANCE

// MAKE DEPOSIT

// MAKE WITHDRAW

// MAKE TRANSFER

// SHOW TRANSACTIONS