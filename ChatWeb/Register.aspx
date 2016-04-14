<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ChatWeb.Register" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div>
        <h1>
            <span>Muehlbauer Chat</span>
        </h1>
    </div>
    <form id="form1" runat="server">
    <div>
        <div>
            <h2>
                <asp:Label ID="Label2" runat="server" Text="New User Register"></asp:Label></h2>
        </div>
        <div>
            <asp:Label ID="Label1" runat="server" Text="ID" Width="150"></asp:Label>
            <asp:TextBox ID="IdentText" runat="server" OnTextChanged="IdentText_TextChanged"></asp:TextBox>
            <asp:Label ID="ErrorInfoLabel" runat="server" Text="Your emploee ID"></asp:Label>
        </div>
        <div>
            <asp:Label ID="Label5" runat="server" Text="Name" Width="150"></asp:Label>
            <asp:TextBox ID="NameText" runat="server"></asp:TextBox>
        </div>
        <div>
            <asp:Label ID="Label6" runat="server" Text="Gender" Width="150"></asp:Label>
            <asp:DropDownList ID="GenderDropdwon" runat="server">
                <asp:ListItem Value="1">Male</asp:ListItem>
                <asp:ListItem Value="0">Female</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div>
            <asp:Label ID="Label3" runat="server" Text="Password" Width="150"></asp:Label>
            <asp:TextBox ID="NewPasswordText" runat="server" TextMode="Password"></asp:TextBox>
        </div>
        <div>
            <asp:Label ID="Label4" runat="server" Text="Confirm Password" Width="150"></asp:Label>
            <asp:TextBox ID="ConfirmPasswordText" runat="server" TextMode="Password"></asp:TextBox>
        </div>
        <div>
            <asp:Button ID="RegisterButton" runat="server" Text="Register" OnClick="RegisterButton_Click" />
            <asp:Button ID="CancelButton" runat="server" Text="Cancel" />
        </div>
    </div>
    </form>
     <div>
        <h2>
            <asp:Label ID="ResultLabel" runat="server"></asp:Label>
        </h2>
    </div>
</body>
</html>
