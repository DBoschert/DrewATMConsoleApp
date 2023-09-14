

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
using System.Security.Principal;

const string baseurl = "http://localhost:1111";

HttpClient http = new HttpClient();

JsonSerializerOptions joptions = new JsonSerializerOptions() {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};


// RUN ATM LOGIC
while (true) {
    // LOGIN PROMPT
    Console.Write("Enter Card Code: ");
    int cardCode = Convert.ToInt32(Console.ReadLine());
    Console.Write("Enter Pin Code: ");
    int pinCode = Convert.ToInt32(Console.ReadLine());
    var jsonResponse = await CustomerLoginAsync(http, joptions, cardCode, pinCode);
    var customer = jsonResponse.DataReturned as Customer;
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
    }
    // MAKE DEPOSIT
    else if (action == "d") {
        // MakeDeposit method
        
        // make amount be the amount the user puts in
        // make account be the users account

        await Deposit(amount, account, joptions);

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

string GetMenuSelection(Customer customer) {
    string? selection;
    Console.Clear();
    Console.WriteLine($"Hello {customer!.Name}!\n");
    while (true) {
        Console.WriteLine("'b' = Balance");
        Console.WriteLine("'d' = Deposit");
        Console.WriteLine("'w' = Withdraw");
        Console.WriteLine("'t' = Transfer");
        Console.WriteLine("'st' = Show Transactions");
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


// CUSTOMER LOGIN
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

// CHECK BALANCE

// MAKE DEPOSIT

// make amount be the amount the user puts in
// make account be the users account

async Task<JsonResponse> Deposit(decimal amount, Account account, JsonSerializerOptions options)
{
    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/deposit/{amount}/{account.Id}");
    var json = JsonSerializer.Serialize<Account>(account, options);
    req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage res = await http.SendAsync(req);
    Console.WriteLine($"HTTP StatusCode is {res.StatusCode}");
    return new JsonResponse()
    {
        HttpStatusCode = (int)res.StatusCode
    };
}

// MAKE WITHDRAW

// MAKE TRANSFER

// SHOW TRANSACTIONS