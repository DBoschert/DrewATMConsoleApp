using ATMConsoleApp.Models;
using ATMConsoleApp;
using System.Text.Json;

/* -*-*-*-*-*-*-*-*-*-*-*  ATM LOGIC  *-*-*-*-*-*-*-*-*-*-*- */

const string baseurl = "http://localhost:1111";

HttpClient http = new HttpClient();

JsonSerializerOptions joptions = new JsonSerializerOptions() {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

// RUN ATM LOGIC
Customer? customer = await LoginPrompt();
JsonResponse jsonResponse;

while (true) {
    // MAIN MENU
    Console.Clear();
    Console.WriteLine(ShowHeader());
    var action = GetMenuSelection(customer).ToLower();

    // CHECK BALANCE
    if (action == "b") {
        // CheckBalance method
        Account account = await SelectAccount(http, joptions, customer.Id, Msg("Please selection an account: "));
        jsonResponse = await CheckBalance(http, joptions, account.Id);
        Console.WriteLine($"{account.Description} Balance: ${account.Balance}");
        Console.Write("[PRESS ENTER]");
        Console.ReadLine();
        Console.Clear();
    }
    // MAKE DEPOSIT
    else if (action == "d") {
        // Deposit method
        Account account = await SelectAccount(http, joptions, customer.Id, Msg("Please selection an account: "));
        jsonResponse = await Deposit(500, account, http, joptions);
        Console.WriteLine("Transfer Successful!");
        Console.Write("[PRESS ENTER]");
        Console.ReadLine();
        Console.Clear();
        continue;
    }
    // MAKE WITHDRAW
    else if (action == "w") {
        // MakeWithdraw method
        Account account = await SelectAccount(http, joptions, customer.Id, Msg("Please selection an account: "));
        jsonResponse = await Withdraw(200, account, http, joptions);
        Console.WriteLine("Withdraw Successful!");
        Console.Write("[PRESS ENTER]");
        Console.ReadLine();
        Console.Clear();
    }
    // MAKE TRANSFER
    else if (action == "t") {
        // MakeTransfer method
        Account account1 = await SelectAccount(http, joptions, customer.Id, Msg("Please selection FROM account: "));
        Account account2 = await SelectAccount(http, joptions, customer.Id, Msg("Please selection TO account: "));
        jsonResponse = await Transfer(35, account1, account2, http, joptions);
        Console.WriteLine("Transfer Successful!");
        Console.Write("[PRESS ENTER]");
        Console.ReadLine();
        Console.Clear();
    }
    // SHOW TRANSACTIONS
    else if (action == "st") {
        // ShowTransactions method
    } 
    // EXIT ATM APP
    else if (action == "x") break;
}

/* -*-*-*-*-*-*-*-*-*-*-*  LOGIC METHODS  *-*-*-*-*-*-*-*-*-*-*- */

// SHOW HEADER
string ShowHeader() {
    return "+-------------------+\n" +
           "|    ATM MACHINE    |\n" +
           "+-------------------+";
}

// LOGIN PROMPT
async Task<Customer> LoginPrompt() {
    while (true) {
        // LOGIN PROMPT
        Console.WriteLine(ShowHeader());
        Console.Write("Enter Card Code: ");
        int cardCode = Convert.ToInt32(Console.ReadLine());
        Console.Write("Enter Pin Code: ");
        int pinCode = Convert.ToInt32(Console.ReadLine());
        jsonResponse = await CustomerLoginAsync(http, joptions, cardCode, pinCode);
        customer = jsonResponse.DataReturned as Customer;
        var status = jsonResponse.HttpStatusCode;
        if (customer == null || status == 404) {
            Console.WriteLine("[Login Failed!]");
            Console.ReadLine();
            Console.Clear();
            continue;
        }
        break;
    }
    return customer;
}

// PRINT MESSAGE
string Msg(string msg) {
    return $"{msg}";
}

/* -*-*-*-*-*-*-*-*-*-*-*  CALL METHODS  *-*-*-*-*-*-*-*-*-*-*- */

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
    Console.WriteLine($"Hello {customer!.Name}!\n");
    while (true) {
        Console.WriteLine("'B' = Balance");
        Console.WriteLine("'D' = Deposit");
        Console.WriteLine("'W' = Withdraw");
        Console.WriteLine("'T' = Transfer");
        Console.WriteLine("'ST' = Show Transactions");
        Console.WriteLine("'X' = Exit");
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
async Task<Account> SelectAccount(HttpClient http, JsonSerializerOptions joptions, int custId, string msg) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts");
    HttpResponseMessage res = await http.SendAsync(req);
    if (res.StatusCode != System.Net.HttpStatusCode.OK) {
        Console.WriteLine($"Http ErrorCode: {res.StatusCode}");
    }
    var json = await res.Content.ReadAsStringAsync();
    var accounts = JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), joptions) as IEnumerable<Account>;
    // SELECTION MENU
    Console.Clear();
    Console.WriteLine(ShowHeader());
    Console.WriteLine(" ACCOUNTS: \n" +
                      "----------");
    var custAccounts = accounts!.Where(x => x.CustomerId == custId);
    foreach(Account acct in custAccounts) {
        Console.WriteLine($"'{acct.Id}' {acct.Description}");
    }
    Console.Write($"\n{msg}");
    var acctId = Convert.ToInt32(Console.ReadLine());
    var account = accounts!.Where(x => x.Id == acctId).SingleOrDefault();
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

// DEPOSIT
async Task<JsonResponse> Deposit(decimal amount, Account account, HttpClient http, JsonSerializerOptions joptions) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/deposit/{amount}/{account.Id}");
    var json = JsonSerializer.Serialize<Account>(account, joptions);
    req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    return new JsonResponse() {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = account
    };
}

// WITHDRAW
async Task<JsonResponse> Withdraw(decimal amount, Account account, HttpClient http, JsonSerializerOptions joptions) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/withdraw/{amount}/{account.Id}");
    var json = JsonSerializer.Serialize<Account>(account, joptions);
    req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    return new JsonResponse() {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = account
    };
}

// TRANSFER
async Task<JsonResponse> Transfer(decimal amount, Account account1, Account account2, HttpClient http, JsonSerializerOptions joptions) {
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/transfer/{amount}/{account1.Id}/{account2.Id}");
    var json1 = JsonSerializer.Serialize(account1, joptions);
    var json2 = JsonSerializer.Serialize(account2, joptions);
    req.Content = new StringContent(json1, System.Text.Encoding.UTF8, "application/json");
    req.Content = new StringContent(json2, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    return new JsonResponse() {
        HttpStatusCode = (int)res.StatusCode,
        DataReturned = account2
    };
}

// SHOW TRANSACTIONS