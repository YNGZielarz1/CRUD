using MySqlConnector;

namespace APIv2
{
    public partial class MainPage : ContentPage
    {
        DatabaseService dbService;

        public MainPage()
        {
            InitializeComponent();
            dbService = new DatabaseService();
            InitializeDatabaseConnection(); // Inicjalizuj połączenie z bazą danych
        }

        private async void InitializeDatabaseConnection()
        {
            await dbService.ConnectToDatabase();
        }

        // Obsługa kliknięcia "Dodaj użytkownika"
        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            string name = nameEntry.Text;
            string place = placeEntry.Text;
            string location = locationEntry.Text;
            string responsiblePerson = responsiblePersonEntry.Text;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(place))
            {
                await dbService.AddAsset(name, location, place, responsiblePerson);
                await DisplayAlert("Sukces", "Użytkownik został dodany", "OK");
            }
            else
            {
                await DisplayAlert("Błąd", "Wszystkie pola muszą być wypełnione", "OK");
            }
        }

        // Obsługa kliknięcia "Wyświetl użytkowników"
        private async void OnGetUsersClicked(object sender, EventArgs e)
        {
            List<User> users = await dbService.GetUsers(); // Pobierz listę użytkowników z bazy danych
            usersCollectionView.ItemsSource = users; // Przypisz listę użytkowników do CollectionView
        }


        // Nowa funkcja obsługująca kliknięcie "Wyświetl użytkownika"
        private async void OnGetUserClicked(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(idEntry.Text, out id))
            {
                // Szukamy użytkownika po ID
                var asset = await dbService.GetAssetById(id);
                if (asset != null)
                {
                    // Wyświetlamy dane użytkownika
                    userIdLabel.Text = $"ID: {asset.Id}";
                    userNameLabel.Text = $"Nazwa: {asset.Name}";
                    userPlaceLabel.Text = $"Miejsce: {asset.Place}";
                    userLocationLabel.Text = $"Lokalizacja: {asset.Location}";
                    userResponsiblePersonLabel.Text = $"Osoba odpowiedzialna: {asset.ResponsiblePerson}";

                    // Pokazujemy sekcję z detalami
                    userDetailsFrame.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie znaleziono użytkownika o podanym ID", "OK");
                    userDetailsFrame.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Błąd", "Wprowadź poprawny numer ID", "OK");
            }
        }

        // Obsługa kliknięcia "Edytuj użytkownika"
        private async void OnEditUserClicked(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(idEntry.Text, out id))
            {
                string name = nameEntry.Text;
                string place = placeEntry.Text;
                string location = locationEntry.Text;
                string responsiblePerson = responsiblePersonEntry.Text;

                await dbService.UpdateAsset(id, name, location, place, responsiblePerson);
                await DisplayAlert("Sukces", $"Użytkownik {id} został zaktualizowany", "OK");
            }
            else
            {
                await DisplayAlert("Błąd", "Wprowadź poprawny numer ID", "OK");
            }
        }

        // Edycja użytkownika wyświetlonego w szczegółach
        private async void OnEditUserClickedFromDetails(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(userIdLabel.Text.Replace("ID: ", ""), out id))
            {
                string name = userNameLabel.Text.Replace("Nazwa: ", "");
                string place = userPlaceLabel.Text.Replace("Miejsce: ", "");
                string location = userLocationLabel.Text.Replace("Lokalizacja: ", "");
                string responsiblePerson = userResponsiblePersonLabel.Text.Replace("Osoba odpowiedzialna: ", "");

                await dbService.UpdateAsset(id, name, location, place, responsiblePerson);
                await DisplayAlert("Sukces", $"Użytkownik {id} został zaktualizowany", "OK");
            }
            else
            {
                await DisplayAlert("Błąd", "Nieprawidłowe ID", "OK");
            }
        }

        // Obsługa kliknięcia "Usuń użytkownika"
        private async void OnDeleteUserClicked(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(idEntry.Text, out id))
            {
                await dbService.DeleteAsset(id);
                await DisplayAlert("Sukces", $"Użytkownik {id} został usunięty", "OK");
            }
            else
            {
                await DisplayAlert("Błąd", "Wprowadź poprawny numer ID", "OK");
            }
        }

        // Usunięcie użytkownika wyświetlonego w szczegółach
        private async void OnDeleteUserClickedFromDetails(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(userIdLabel.Text.Replace("ID: ", ""), out id))
            {
                await dbService.DeleteAsset(id);
                await DisplayAlert("Sukces", $"Użytkownik {id} został usunięty", "OK");
                userDetailsFrame.IsVisible = false; // Ukryj dane po usunięciu
            }
            else
            {
                await DisplayAlert("Błąd", "Nieprawidłowe ID", "OK");
            }
        }
    }

    // Klasa reprezentująca użytkownika
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Place { get; set; }
        public string ResponsiblePerson { get; set; }
    }

    // Klasa odpowiedzialna za komunikację z bazą danych
    public class DatabaseService
    {
        private string connectionString = "server=localhost;user id=root;password=L1nUx12345678910;database=DB2";

        public async Task ConnectToDatabase()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Połączenie udane!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd połączenia: {ex.Message}");
                }
            }
        }

        // Pobierz listę użytkowników z bazy danych
        public async Task<List<User>> GetUsers()
        {
            List<User> users = new List<User>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM assets", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Location = reader.GetString(2),
                            Place = reader.GetString(3),
                            ResponsiblePerson = reader.GetString(4)
                        });
                    }
                }
            }

            return users;
        }

        public async Task AddAsset(string name, string location, string place, string responsiblePerson)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("INSERT INTO assets (name, location, place, responsible_person) VALUES (@name, @location, @place, @responsiblePerson)", connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@location", location);
                    command.Parameters.AddWithValue("@place", place);
                    command.Parameters.AddWithValue("@responsiblePerson", responsiblePerson);

                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("Nowy rekord został dodany.");
                }
            }
        }

        public async Task UpdateAsset(int id, string name, string location, string place, string responsiblePerson)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("UPDATE assets SET name = @name, location = @location, place = @place, responsible_person = @responsiblePerson WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@location", location);
                    command.Parameters.AddWithValue("@place", place);
                    command.Parameters.AddWithValue("@responsiblePerson", responsiblePerson);

                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Rekord o ID {id} został zaktualizowany.");
                }
            }
        }

        public async Task DeleteAsset(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("DELETE FROM assets WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Rekord o ID {id} został usunięty.");
                }
            }
        }

        // Pobierz użytkownika na podstawie ID
        public async Task<User> GetAssetById(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM assets WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Location = reader.GetString(2),
                                Place = reader.GetString(3),
                                ResponsiblePerson = reader.GetString(4)
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
