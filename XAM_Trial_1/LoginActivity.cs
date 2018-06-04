using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Data.SqlClient;

namespace XAM_Trial_1
{
	[Activity(Label = "Login Activity", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme", NoHistory = true)]
	public class LoginActivity : Activity
	{
		string userID;
        string password;
		string retrievedPassword;
		string[] positions;
		SqlConnection connection;
        SqlCommand command;
        SqlDataReader dataReader;
		bool loggedIn;

        protected override void OnCreate(Bundle savedInstanceState)
		{
			if (loggedIn == true) StartMainActivity();

			else
			{
				base.OnCreate(savedInstanceState);

				SetContentView(Resource.Layout.Login);

				SetUpDatabaseConnection();

				Button loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
				loginBtn.Click += LoginBtn_Clicked;
			}
		}

		private void SetUpDatabaseConnection()
		{
			string connectionString = "Data Source=67.173.30.52,1433;" +
										"Network Library=DBMSSOCN;Initial Catalog=wsmobile_db;" +
										"User ID=inder;Password=Peterbawa96";
			connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
				var toast = Toast.MakeText(this, "Connected!", ToastLength.Short);
				toast.Show();
				//string sql = $"SELECT [password], [positions] FROM user_positions WHERE [user]='{userID}'";
				//command = new SqlCommand(sql, connection);
				//dataReader = command.ExecuteReader();
				//while (dataReader.Read())
				//{
				//    var dataToast = Toast.MakeText(this, dataReader.GetValue(0).ToString(), ToastLength.Long);
				//    dataToast.Show();
				//}
			}
			catch (Exception E)
			{
				var toast = Toast.MakeText(this, E.Message, ToastLength.Long);
				toast.Show();
			}
		}

		private void LoginBtn_Clicked(object sender, EventArgs e)
        {
            userID = FindViewById<EditText>(Resource.Id.userID).Text;
            password = FindViewById<EditText>(Resource.Id.password).Text;

            string sql = $"SELECT [password], [positions] FROM user_positions WHERE [user]='{userID}'";
            command = new SqlCommand(sql, connection);
            dataReader = command.ExecuteReader();
            dataReader.Read();
			try
			{
				retrievedPassword = dataReader.GetValue(0).ToString();
				if (password == retrievedPassword.Trim())
				{
					positions = dataReader.GetValue(1).ToString().Split(',');
					Toast.MakeText(this, "Access Granted", ToastLength.Short).Show();
					this.loggedIn = true;
					StartMainActivity();
					connection.Close();
				}
			}
			catch(InvalidOperationException)
			{
				Toast.MakeText(this, "Could not find an account with that User Name", ToastLength.Long).Show();
			}
            Toast.MakeText(this, $"Retrieved password was '{retrievedPassword}', while given password was '{password}'", ToastLength.Short).Show();
            
            dataReader.Close();
        }

        async void StartMainActivity()
		{
			await Task.Run(() => {
				Intent MainActivityIntent = new Intent(this, typeof(MainActivity));
				Bundle extras = new Bundle();
				extras.PutString("userId", userID);
				extras.PutString("password", password);
				extras.PutStringArray("positions", positions);
				MainActivityIntent.PutExtras(extras);
				StartActivity(MainActivityIntent);
			});
		}

    }
}