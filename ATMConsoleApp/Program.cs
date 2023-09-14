

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
JsonResponse jsonResponse;

// RUN ATM LOGIC
while (true) {
    // LOGIN PROMPT
    Console.Write("Enter Card Code: ");
    int cardCode = Convert.ToInt32(Console.ReadLine());
    Console.Write("Enter Pin Code: ");
    int pinCode = Convert.ToInt32(Console.ReadLine());
    jsonResponse = await CustomerLoginAsync(http, joptions, cardCode, pinCode);
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
        // CheckBalance method
        Account account = await SelectAccount(http, joptions, customer.Id);
        jsonResponse = await CheckBalance(http, joptions, account.Id);
        Console.WriteLine($"{account.Description} Balance: ${account.Balance}");
    }
    // MAKE DEPOSIT
    else if (action == "d") {
        // Deposit method
        Account account = await SelectAccount(http, joptions, customer.Id);
        jsonResponse = await Deposit(500, account, http, joptions);
        Console.WriteLine("Transfer Successful!");
    }
    // MAKE WITHDRAW
    else if (action == "w") {
        // MakeWithdraw method
        Account account = await SelectAccount(http, joptions, customer.Id);
        jsonResponse = await Withdraw(200, account, http, joptions);
        Console.WriteLine("Withdraw Successful!");
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
    Console.Write("\nPlease selection an account: ");
    var acctId = Convert.ToInt32(Console.ReadLine());
    var account = accounts.Where(x => x.Id == acctId).SingleOrDefault();
    return account!;
}


// CHECK BALANCE
async Task<JsonResponse> CheckBalance(HttpClient http, JsonSerializerOptions joptions, int acctId) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts/balance/{acctId}");
    HttpResponseMessage res = await http.SendAsync(req);
    var balance = await res.Content.ReadAsStringAsync();
    return new JsonResponse {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = balance.ToString()
    };
}

// MAKE DEPOSIT

// make amount be the amout the user puts in
// make account be the users account

async Task<JsonResponse> Deposit(decimal amount, Account account, HttpClient http, JsonSerializerOptions options) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/deposit/{amount}/{account.Id}");
    var json = JsonSerializer.Serialize<Account>(account, options);
    req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    return new JsonResponse() {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = account
    };
}

// MAKE WITHDRAW
async Task<JsonResponse> Withdraw(decimal amount, Account account, HttpClient http, JsonSerializerOptions options) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/withdraw/{amount}/{account.Id}");
    var json = JsonSerializer.Serialize<Account>(account, options);
    req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    return new JsonResponse() {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = account
    };
}

// MAKE TRANSFER

// SHOW TRANSACTIONS