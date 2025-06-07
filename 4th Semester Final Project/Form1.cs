using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ScottPlot;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using Color = ScottPlot.Color;
using Microsoft.VisualBasic.FileIO;

namespace _4th_Semester_Final_Project
{
    public partial class Form1 : Form
    {
        private DataTable originalDataTable;
        private string currentFilePath;
        private string currentFileExtension;
        private const string apiKey = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIyMWI2MmYxZmU3MGY0Nzg5YzFhNWIyMzQ1OWE4Y2EyNyIsIm5iZiI6MTc0NzUzNDE3Ny41NjQ5OTk4LCJzdWIiOiI2ODI5NDE2MTA4MjQ5ZDQ4MTkwYmQyY2YiLCJzY29wZXMiOlsiYXBpX3JlYWQiXSwidmVyc2lvbiI6MX0.u57FCdaBczX-UG2oEkrcMiBcU4nAIHaskxQLCmtNUxM"; // Reemplázalo con tu clave de Last.fm
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private string connectionString = @"Server=localhost\SQLEXPRESS;Database=TopChessPlayer;Trusted_connection=yes; TrustServerCertificate=true";
        private string dataSourceType = "";

        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            cmbMovieType.Items.AddRange(new string[] { "Popular", "Top Rated", "Now Playing", "Upcoming" });
            cmbMovieType.SelectedIndex = -1;
        }

        private void InitializeDatabase()
        {
            try
            {
                connection = new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}");
            }
        }

        private async void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported files|*.csv;*.xml;*.json;*.txt|CSV (*.csv)|*.csv|XML (*.xml)|*.xml|JSON (*.json)|*.json|Texto (*.txt)|*.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = ofd.FileName;
                    string ext = Path.GetExtension(filePath).ToLower();

                    try
                    {
                        ClearChart();
                        cmbMovieType.Visible = false;
                        dgvData.DataSource = null;

                        switch (ext)
                        {
                            case ".csv":
                                dgvData.DataSource = await LoadDataFromCSVAsync(filePath);
                                break;
                            case ".txt":
                                // Asumimos que los .txt usan | como delimitador
                                dgvData.DataSource = LoadDataFromTXT(filePath, '|');
                                break;
                            case ".xml":
                                dgvData.DataSource = LoadDataFromXML(filePath);
                                break;
                            case ".json":
                                dgvData.DataSource = LoadDataFromJSON(filePath);
                                break;
                            default:
                                break;
                        }
                        dataSourceType = ext switch
                        {
                            ".csv" => "CSV",
                            ".txt" => "TXT",
                            ".xml" => "XML",
                            ".json" => "JSON",
                            _ => ""
                        };
                        cmbFilter.Items.Clear();
                        foreach (DataColumn col in originalDataTable.Columns)
                        {
                            cmbFilter.Items.Add(col.ColumnName);
                        }
                        UpdateRecordCount();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al procesar los datos: " + ex.Message);
                    }
                }
            }
        }

        private async void LoadInTextbox(string filePath)
        {
            txtData.Text = "Cargando...";

            string texto = await Task.Run(() => File.ReadAllText(filePath, Encoding.Latin1));

            txtData.Text = texto;
        }

        private async Task<DataTable> LoadDataFromCSVAsync(string filePath)
        {
            var dataTable = new DataTable();
            string[] headers = null;

            using (var reader = new StreamReader(filePath))
            {
                string line = await reader.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    headers = SplitCsvLine(line);
                    foreach (string header in headers)
                        dataTable.Columns.Add(header);
                }

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] fields = SplitCsvLine(line);

                    if (fields == null || fields.Length < headers.Length)
                        continue;

                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dataRow[i] = fields[i];
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            currentFilePath = filePath;
            currentFileExtension = Path.GetExtension(filePath).ToLower();
            originalDataTable = dataTable;

            return dataTable;
        }

        // Función simple para dividir líneas CSV (mejor que TextFieldParser para rendimiento)
        private string[] SplitCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            int fieldStart = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    if (i + 1 < line.Length && line[i + 1] == '"') // Escapar ""
                    {
                        line = line.Remove(i + 1, 1); // Eliminar un "
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(line.Substring(fieldStart, i - fieldStart));
                    fieldStart = i + 1;
                }
            }

            result.Add(line.Substring(fieldStart));

            return result.ToArray();
        }

        private DataTable LoadDataFromJSON(string filePath)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (File.Exists(filePath))
                {
                    // Leer el contenido del archivo JSON
                    string jsonContent = File.ReadAllText(filePath);

                    // Deserializar el JSON a una lista de objetos anónimos
                    var dataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);

                    // Verificar si dataList tiene elementos
                    if (dataList != null && dataList.Count > 0)
                    {
                        // Agregar columnas al DataTable basado en las claves del primer diccionario
                        foreach (var key in dataList[0].Keys)
                        {
                            dataTable.Columns.Add(key);
                        }

                        // Agregar filas al DataTable
                        foreach (var data in dataList)
                        {
                            DataRow row = dataTable.NewRow();
                            foreach (var key in data.Keys)
                            {
                                row[key] = data[key]?.ToString(); // Convertir valores a cadena
                            }
                            dataTable.Rows.Add(row);
                        }
                        currentFilePath = filePath;
                        currentFileExtension = Path.GetExtension(filePath).ToLower();

                        originalDataTable = dataTable;
                    }
                    else
                    {
                        MessageBox.Show($"The JSON file \"{Path.GetFileName(filePath)}\" está vacío o no tiene el formato esperado.");
                    }
                }
                else
                {
                    MessageBox.Show($"The JSON file \"{Path.GetFileName(filePath)}\" no existe.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading JSON file \"{Path.GetFileName(filePath)}\": {ex.Message}");
            }
            return dataTable;
        }

        private DataTable LoadDataFromXML(string filePath)
        {
            DataTable dataTable = new DataTable();
            try
            {
                // Leer el archivo XML en un DataSet
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(filePath);

                // Verificar si el DataSet tiene al menos una tabla y esta tiene al menos una fila
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    // Asignar la tabla del DataSet al DataTable
                    dataTable = dataSet.Tables[0];
                }
                currentFilePath = filePath;
                currentFileExtension = Path.GetExtension(filePath).ToLower();

                originalDataTable = dataTable;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading XML file: " + ex.Message);
            }
            return dataTable;
        }

        private DataTable LoadDataFromTXT(string filePath, char delimiter)
        {
            DataTable dt = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string[] headers = sr.ReadLine().Split(delimiter);
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(delimiter);
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }
                }
                currentFilePath = filePath;
                currentFileExtension = Path.GetExtension(filePath).ToLower();

                originalDataTable = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading TXT file: " + ex.Message);
            }
            return dt;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            switch (dataSourceType)
            {
                case "API":
                    break;
                case "CSV":
                case "XML":
                case "JSON":
                    if (string.IsNullOrEmpty(currentFilePath) || originalDataTable == null)
                    {
                        MessageBox.Show("No file uploaded to save.");
                        return;
                    }

                    switch (currentFileExtension)
                    {
                        case ".csv":
                            ExportToCSV(currentFilePath);
                            break;
                        case ".txt":
                            ExportToTxt(currentFilePath, '|');
                            break;
                        case ".json":
                            ExportToJson(currentFilePath);
                            break;
                        case ".xml":
                            ExportToXML(currentFilePath);
                            break;
                        default:
                            MessageBox.Show("Unsupported format for saving.");
                            break;
                    }

                    MessageBox.Show("File saved successfully.");
                    break;

                case "DATABASE":
                    try
                    {
                        if (dataSourceType != "DATABASE")
                        {
                            MessageBox.Show("No data has been loaded from the database. Please load data first.");
                            return;
                        }

                        if (originalDataTable != null)
                        {
                            // Aplicar cambios pendientes en el DataGridView
                            dgvData.EndEdit();

                            // Actualizar la base de datos
                            adapter.Update(originalDataTable);

                            MessageBox.Show("Changes saved successfully");
                        }
                        else
                        {
                            MessageBox.Show("There is no data to save. Please load data first.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving changes: {ex.Message}");
                    }
                    break;

                default:
                    MessageBox.Show("No data source selected.");
                    break;
            }

        }

        private void ExportToCSV(string filepath)
        {
            using (StreamWriter writer = new StreamWriter(filepath))
            {
                // Escribir encabezados
                StringBuilder filaEncabezados = new StringBuilder();
                foreach (DataGridViewColumn columna in dgvData.Columns)
                {
                    filaEncabezados.Append("\"" + columna.HeaderText.Replace("\"", "\"\"") + "\",");
                }
                writer.WriteLine(filaEncabezados.ToString().TrimEnd(','));

                // Escribir filas
                foreach (DataGridViewRow fila in dgvData.Rows)
                {
                    StringBuilder filaDatos = new StringBuilder();
                    foreach (DataGridViewCell celda in fila.Cells)
                    {
                        if (celda.Value != null)
                        {
                            string valor = celda.Value.ToString();
                            // Si el valor contiene comillas o comas, lo encerramos entre comillas
                            if (valor.Contains(",") || valor.Contains("\""))
                            {
                                valor = "\"" + valor.Replace("\"", "\"\"") + "\"";
                            }
                            filaDatos.Append(valor + ",");
                        }
                        else
                        {
                            filaDatos.Append(",");
                        }
                    }
                    writer.WriteLine(filaDatos.ToString().TrimEnd(','));
                }
            }
        }

        private void ExportToXML(string filePath)
        {
            try
            {
                DataTable dataTable = (DataTable)dgvData.DataSource;

                if (dataTable != null)
                {
                    if (string.IsNullOrEmpty(dataTable.TableName))
                    {
                        dataTable.TableName = "data";
                    }

                    // Guardar el DataTable como XML en el archivo seleccionado por el usuario
                    //string filePath = saveFileDialog1.FileName;
                    dataTable.WriteXml(filePath, XmlWriteMode.WriteSchema);
                }
                else
                {
                    MessageBox.Show("There is no data in the DataGridView to save.");
                }
            }
            catch (Exception ex)
            {
                // Manejar errores durante el proceso de guardado
                MessageBox.Show("Error saving XML file: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void ExportToJson(string filePath)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (!row.IsNewRow)
                {
                    var rowDict = new Dictionary<string, object>();

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string header = dgvData.Columns[cell.ColumnIndex].HeaderText;
                        object value = cell.Value ?? "[EMPTY CELL]";
                        rowDict[header] = value;
                    }

                    rows.Add(rowDict);
                }
            }

            // Serialize to JSON with indentation
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(rows, Newtonsoft.Json.Formatting.Indented);

            // Save to file
            File.WriteAllText(filePath, json);
        }

        private void ExportToTxt(string filePath, char separator = '|')
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Escribir los encabezados
                StringBuilder headerLine = new StringBuilder();
                foreach (DataGridViewColumn column in dgvData.Columns)
                {
                    headerLine.Append(column.HeaderText).Append(separator);
                }
                writer.WriteLine(headerLine.ToString().TrimEnd(separator));

                // Escribir las filas
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        StringBuilder rowLine = new StringBuilder();

                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            string value = row.Cells[i].Value?.ToString() ?? "";
                            rowLine.Append(value).Append(separator);
                        }

                        // Dejar la línea tal cual, sin TrimEnd(separator) al final
                        writer.WriteLine(rowLine.ToString());
                    }
                }
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (originalDataTable == null || cmbFilter.SelectedItem == null)
                return;

            string selectedColumn = cmbFilter.SelectedItem.ToString();
            string filterText = txtFilter.Text.Trim();

            DataView dv = new DataView(originalDataTable);
            var columnType = originalDataTable.Columns[selectedColumn].DataType;

            try
            {
                if (string.IsNullOrEmpty(filterText))
                {
                    dgvData.DataSource = originalDataTable;
                }
                else
                {
                    if (columnType == typeof(string))
                    {
                        filterText = filterText.Replace("'", "''");
                        dv.RowFilter = $"[{selectedColumn}] LIKE '%{filterText}%'";
                    }
                    else if (columnType == typeof(int) || columnType == typeof(long) ||
                             columnType == typeof(short) || columnType == typeof(byte))
                    {
                        if (int.TryParse(filterText, out int intValue))
                        {
                            dv.RowFilter = $"[{selectedColumn}] = {intValue}";
                        }
                        else
                        {
                            dv.RowFilter = "1 = 0";
                        }
                    }
                    else if (columnType == typeof(double) || columnType == typeof(float) ||
                             columnType == typeof(decimal))
                    {
                        if (double.TryParse(filterText, out double doubleValue))
                        {
                            dv.RowFilter = $"[{selectedColumn}] = {doubleValue}";
                        }
                        else
                        {
                            dv.RowFilter = "1 = 0";
                        }
                    }
                    else if (columnType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(filterText, out DateTime dateValue))
                        {
                            dv.RowFilter = $"[{selectedColumn}] = #{dateValue:MM/dd/yyyy}#";
                        }
                        else
                        {
                            dv.RowFilter = "1 = 0";
                        }
                    }
                    else
                    {
                        filterText = filterText.Replace("'", "''");
                        dv.RowFilter = $"CONVERT([{selectedColumn}], 'System.String') LIKE '%{filterText}%'";
                    }


                }

                dgvData.DataSource = dv;

                if (dataSourceType == "API")
                {
                    List<Movie> filteredMovies = ConvertDataTableToMovies(dv.ToTable());
                    ShowLanguageDistribution(filteredMovies);
                    LoadMoviesIntoTreeView(filteredMovies);
                }
                else if (dataSourceType == "CSV" || dataSourceType == "XML" || dataSourceType == "JSON" || dataSourceType == "TXT")
                {
                    LoadPlayerStatsIntoTreeView(dv);
                }
                else if (dataSourceType == "DATABASE")
                {
                    LoadPlayerChessStatsIntoTreeView(dv);
                    GenerateGraph();
                }
                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying filter: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvData.Rows.Count <= 0)
            {
                MessageBox.Show("There are no data to export. Please load data first.");
                return;
            }

            SaveFileDialog dialogoGuardar = new SaveFileDialog();
            dialogoGuardar.Filter = "Supported files|*.csv;*.xml;*.json;*.txt|CSV (*.csv)|*.csv|XML (*.xml)|*.xml|JSON (*.json)|*.json|Texto (*.txt)|*.txt";
            dialogoGuardar.Filter += "|PDF (*.pdf)|*.pdf";
            dialogoGuardar.DefaultExt = "csv";

            if (dialogoGuardar.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialogoGuardar.FileName;
                string ext = Path.GetExtension(filePath).ToLower();

                try
                {
                    switch (ext)
                    {
                        case ".csv":
                            ExportToCSV(filePath);
                            break;
                        case ".txt":
                            ExportToTxt(filePath, '|');
                            break;
                        case ".xml":
                            ExportToXML(filePath);
                            break;
                        case ".json":
                            ExportToJson(filePath);
                            break;
                        case ".pdf":
                            ExportToPdf(filePath);
                            break;
                        default:
                            MessageBox.Show("Unsupported format.");
                            return;
                    }

                    MessageBox.Show("File exported successfully");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message);
                }
            }
        }

        private void ExportToPdf(string filePath)
        {
            // Cambiar a orientación horizontal usando Rotate()
            Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Fuente base
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.NORMAL);

                // Crear la tabla con el número de columnas del DataGridView
                PdfPTable pdfTable = new PdfPTable(dgvData.Columns.Count);
                pdfTable.WidthPercentage = 100;

                // Agregar encabezados
                foreach (DataGridViewColumn column in dgvData.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, font));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    pdfTable.AddCell(cell);
                }

                // Agregar filas
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string valor = cell.Value?.ToString() ?? string.Empty;

                            // Limpiar o formatear si hay caracteres problemáticos (opcional)
                            // Ejemplo: Reemplazar múltiples espacios o saltos de línea si fuera necesario
                            valor = valor.Replace("\n", " ").Replace("\r", "").Trim();

                            pdfTable.AddCell(new Phrase(valor, font));
                        }
                    }
                }

                doc.Add(pdfTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
            finally
            {
                doc.Close();
            }
        }

        private void btnEmail_Click(object sender, EventArgs e)
        {
            string recipient = txtAddressee.Text.Trim();

            if (string.IsNullOrEmpty(recipient))
            {
                MessageBox.Show("Please enter the recipient email address.");
                return;
            }

            // Mostrar diálogo para seleccionar tipo de archivo
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Select file type to send";
                sfd.Filter = "CSV File (*.csv)|*.csv|TXT File (*.txt)|*.txt|JSON File (*.json)|*.json|XML File (*.xml)|*.xml|PDF File (*.pdf)|*.pdf";
                sfd.DefaultExt = "csv";
                sfd.FileName = "ExportedData"; // Nombre por defecto

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string tempFilePath = sfd.FileName;
                    string extension = Path.GetExtension(tempFilePath).ToLower();

                    try
                    {
                        // Exportar los datos actuales (filtrados si hay filtro aplicado)
                        DataTable dataToExport = GetCurrentDataTable();

                        if (dataToExport == null || dataToExport.Rows.Count == 0)
                        {
                            MessageBox.Show("No data to send.");
                            return;
                        }

                        // Exportar a archivo temporal
                        switch (extension)
                        {
                            case ".csv":
                                ExportToCSV(tempFilePath);
                                break;
                            case ".txt":
                                ExportToTxt(tempFilePath, '|');
                                break;
                            case ".json":
                                ExportToJson(tempFilePath);
                                break;
                            case ".xml":
                                ExportToXML(tempFilePath);
                                break;
                            case ".pdf":
                                ExportToPdf(tempFilePath);
                                break;
                            default:
                                MessageBox.Show("Unsupported file type.");
                                return;
                        }

                        // Enviar el archivo por email
                        SendEmailWithAttachment(recipient, tempFilePath);

                        // Eliminar el archivo temporal después de enviarlo
                        File.Delete(tempFilePath);

                        MessageBox.Show("Email sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error sending email: " + ex.Message);
                        // Asegurarse de eliminar el archivo temporal en caso de error
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                }
            }
        }
        private DataTable GetCurrentDataTable()
        {
            // Obtener los datos actuales (filtrados si hay filtro aplicado)
            if (dgvData.DataSource is DataTable dt)
            {
                return dt;
            }
            else if (dgvData.DataSource is DataView dv)
            {
                return dv.ToTable();
            }
            else if (dgvData.DataSource is BindingSource bs && bs.DataSource is DataTable)
            {
                return (DataTable)bs.DataSource;
            }
            else if (originalDataTable != null)
            {
                return originalDataTable;
            }

            return null;
        }

        private void SendEmailWithAttachment(string recipient, string filePath)
        {
            // Configuración SMTP (Gmail)
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("elcomparosh97@gmail.com", "kecp sfqy aosh jpjj"),
                EnableSsl = true
            };

            MailMessage mail = new MailMessage
            {
                From = new MailAddress("elcomparosh97@gmail.com"),
                //Subject = "Data export",
                //Body = "Please find attached the requested data file.",
                IsBodyHtml = false
            };

            mail.To.Add(recipient);
            mail.Attachments.Add(new Attachment(filePath));

            switch (dataSourceType)
            {
                case "API":
                    switch (cmbMovieType.SelectedItem?.ToString())
                    {
                        case "Popular":
                            mail.Subject = "Popular Movies Exported";
                            mail.Body = "I attach the file with the popular films selected.";
                            break;
                        case "Top Rated":
                            mail.Subject = "Top Rated Movies Exported";
                            mail.Body = "I attach the file with the best valued movies.";
                            break;
                        case "Now Playing":
                            mail.Subject = "Now Playing Movies Exported";
                            mail.Body = "I attach the file with the selected billboard films.";
                            break;
                        case "Upcoming":
                            mail.Subject = "Upcoming Movies Exported";
                            mail.Body = "I attach the file with the close films to be released selected.";
                            break;
                        default:
                            break;
                    }
                    break;
                case "DATABASE":
                    mail.Subject = "Top Chess Player Exported";
                    mail.Body = "I attach the file with the best chess players of the month of August 2020.";
                    break;
                case "CSV":
                    mail.Subject = "Footbal Player Stats Exported";
                    mail.Body = "I attach the file with the statistics of soccer players.";
                    
                    break;
                default:
                    mail.Subject = "Datos Exportados";
                    mail.Body = "Por favor, encuentra adjunto el archivo con los datos solicitados.";
                    break;
            }

            client.Send(mail);
            mail.Dispose();
        }

        private async void btnLoadToAPI_Click(object sender, EventArgs e)
        {
            dataSourceType = "API";
            ClearChart();
            cmbMovieType.Visible = true;
            string selectedType = cmbMovieType.SelectedItem?.ToString();
            await LoadMoviesByTypeAsync(selectedType);
        }

        private async Task LoadMoviesByTypeAsync(string movieType)
        {
            if (string.IsNullOrEmpty(movieType))
                return;

            int totalPages = 10;
            var allMovies = new List<Movie>();

            string baseUrl = "https://api.themoviedb.org/3/movie/";
            string endpoint = movieType switch
            {
                "Popular" => "popular",
                "Top Rated" => "top_rated",
                "Now Playing" => "now_playing",
                "Upcoming" => "upcoming",
                _ => "popular"
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    txtData.Text = "Loading...";
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    for (int page = 1; page <= totalPages; page++)
                    {
                        string url = $"{baseUrl}{endpoint}?language=en-US&page={page}";

                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        string json = await response.Content.ReadAsStringAsync();
                        var resultado = JsonConvert.DeserializeObject<TmdbPopularResponse>(json);

                        allMovies.AddRange(resultado.results);
                    }

                    // Actualizar controles del formulario
                    dgvData.DataSource = ConvertToDataTable(allMovies);
                    dataSourceType = "API";
                    ShowLanguageDistribution(allMovies);
                    LoadMoviesIntoTreeView(allMovies);

                    // Mostrar datos en txtData
                    StringBuilder sb = new StringBuilder();
                    foreach (var movie in allMovies)
                    {
                        sb.AppendLine($"ID: {movie.id}");
                        sb.AppendLine($"Title: {movie.title}");
                        sb.AppendLine($"Release Date: {movie.release_date.ToShortDateString()}");
                        sb.AppendLine($"Vote Average: {movie.vote_average}");
                        sb.AppendLine($"Original Language: {movie.original_language}");
                        sb.AppendLine(new string('-', 40));
                    }

                    txtData.Text = sb.ToString();
                    UpdateRecordCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting data: " + ex.Message);
                }
            }
        }

        private DataTable ConvertToDataTable(List<Movie> movies)
        {
            DataTable table = new DataTable();

            // Añadir columnas según las propiedades del objeto Movie
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Release Date", typeof(DateTime));
            table.Columns.Add("Vote Average", typeof(double));
            table.Columns.Add("Overview", typeof(string));
            table.Columns.Add("Popularity", typeof(double));
            table.Columns.Add("Vote Count", typeof(int));
            table.Columns.Add("Poster Path", typeof(string));
            table.Columns.Add("Backdrop Path", typeof(string));
            table.Columns.Add("Original Language", typeof(string));
            table.Columns.Add("Original Title", typeof(string));
            table.Columns.Add("Genre IDs", typeof(string)); // To display as a string
            table.Columns.Add("Adult", typeof(bool));
            table.Columns.Add("Video", typeof(bool));

            // Agregar filas al DataTable
            foreach (var pelicula in movies)
            {
                table.Rows.Add(
                    pelicula.id,
                    pelicula.title,
                    pelicula.release_date,
                    pelicula.vote_average,
                    pelicula.overview,
                    pelicula.popularity,
                    pelicula.vote_count,
                    pelicula.poster_path,
                    pelicula.backdrop_path,
                    pelicula.original_language,
                    pelicula.original_title,
                    string.Join(", ", pelicula.genre_ids), // Convierte lista de IDs a cadena
                    pelicula.adult,
                    pelicula.video
                );
            }
            originalDataTable = table;
            cmbFilter.Items.Clear();
            foreach (DataColumn col in table.Columns)
            {
                cmbFilter.Items.Add(col.ColumnName);
            }
            return table;
        }

        private List<Movie> ConvertDataTableToMovies(DataTable table)
        {
            List<Movie> movies = new List<Movie>();

            foreach (DataRow row in table.Rows)
            {
                var movie = new Movie
                {
                    id = Convert.ToInt32(row["ID"]),
                    title = row["Title"]?.ToString(),
                    release_date = row["Release Date"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(row["Release Date"]),
                    vote_average = Convert.ToDouble(row["Vote Average"]),
                    overview = row["Overview"]?.ToString(),
                    popularity = Convert.ToDouble(row["Popularity"]),
                    vote_count = Convert.ToInt32(row["Vote Count"]),
                    poster_path = row["Poster Path"]?.ToString(),
                    backdrop_path = row["Backdrop Path"]?.ToString(),
                    original_language = row["Original Language"]?.ToString(),
                    original_title = row["Original Title"]?.ToString(),
                    genre_ids = string.IsNullOrEmpty(row["Genre IDs"]?.ToString())
                        ? new List<int>()
                        : row["Genre IDs"].ToString().Split(',').Select(int.Parse).ToList(),
                    adult = Convert.ToBoolean(row["Adult"]),
                    video = Convert.ToBoolean(row["Video"])
                };

                movies.Add(movie);
            }

            return movies;
        }

        private void btnBD_Click(object sender, EventArgs e)
        {
            try
            {
                cmbMovieType.Visible = false;
                ClearChart();
                //string query = "SELECT * FROM top_chess_players_aug_2020";
                string query = "  SELECT TOP 50000 * FROM top_chess_players_aug_2020;";

                adapter = new SqlDataAdapter(query, connection);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);

                originalDataTable = new DataTable();
                adapter.Fill(originalDataTable);

                dgvData.DataSource = originalDataTable;
                cmbFilter.Items.Clear();

                foreach (DataColumn col in originalDataTable.Columns)
                {
                    cmbFilter.Items.Add(col.ColumnName);
                }
                dataSourceType = "DATABASE";
                GenerateGraph();
                UpdateRecordCount();
                MessageBox.Show("Data loaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void DisplayDataTableInTextBox(DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            // Agregar nombres de columnas
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i].ColumnName);
                if (i < table.Columns.Count - 1)
                    sb.Append("\t"); // Separador tabular entre nombres de columnas
            }
            sb.AppendLine();
            sb.AppendLine(new string('-', 80)); // Línea divisoria

            // Agregar filas
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    sb.Append(row[i].ToString());
                    if (i < table.Columns.Count - 1)
                        sb.Append("\t");
                }
                sb.AppendLine();
            }

            txtData.Text = sb.ToString(); // Asignar al TextBox
        }

        private void ShowLanguageDistribution(List<Movie> movies)
        {
            graphic.Plot.Title("");

            var groupedByLanguage = movies
                .GroupBy(m => m.original_language)
                .Select(g => new
                {
                    Language = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            List<Color> availableColors = new List<Color>
            {
                Colors.LightBlue, Colors.LightGreen, Colors.LightCoral,
                Colors.LightSalmon, Colors.LightSkyBlue, Colors.LightPink,
                Colors.Lavender, Colors.Beige, Colors.Bisque,
                Colors.HotPink, Colors.MistyRose, Colors.Tomato
            };

            List<PieSlice> slices = new List<PieSlice>();
            for (int i = 0; i < groupedByLanguage.Count; i++)
            {
                var item = groupedByLanguage[i];
                slices.Add(new PieSlice
                {
                    Value = item.Count,
                    Label = $"{item.Language.ToUpper()} ({item.Count})",
                    FillColor = availableColors[i % availableColors.Count]
                });
            }

            graphic.Plot.Clear();
            var pie = graphic.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.5f;

            graphic.Plot.ShowLegend();
            graphic.Plot.Axes.Frameless();
            graphic.Plot.HideGrid();
            graphic.Plot.Title("Distribution of Movies by Original Language");
            graphic.Refresh();
        }

        private void ClearChart()
        {
            txtData.Clear();
            treeViewMovies.Nodes.Clear();
            cmbFilter.SelectedIndex = -1;
            txtFilter.Clear();
            graphic.Plot.Clear();
            graphic.Plot.Title("");
            graphic.Plot.Axes.Frameless();
            graphic.Plot.HideGrid();
            graphic.Refresh();
            graphic.Plot.Axes.AutoScale();

        }

        private void UpdateRecordCount()
        {
            int rowCount = 0;

            if (dgvData.DataSource is DataTable table)
            {
                rowCount = table.Rows.Count;
            }
            else if (dgvData.DataSource is List<Movie> list)
            {
                rowCount = list.Count;
            }
            else if (dgvData.DataSource is DataView dataView)
            {
                rowCount = dataView.Count;
            }
            if (rowCount == 0)
            {
                lblRecordCount.Text = "No matching records found.";
            }
            else
            {
                lblRecordCount.Text = $"Total records: {rowCount}";
            }
        }

        private void LoadMoviesIntoTreeView(List<Movie> movies)
        {
            treeViewMovies.Nodes.Clear();

            foreach (var movie in movies)
            {
                TreeNode movieNode = new TreeNode(movie.title);

                // Add properties as child nodes
                movieNode.Nodes.Add($"ID: {movie.id}");
                movieNode.Nodes.Add($"Adult: {movie.adult}");
                movieNode.Nodes.Add($"Original Language: {movie.original_language}");
                movieNode.Nodes.Add($"Release Date: {movie.release_date:yyyy-MM-dd}");
                movieNode.Nodes.Add($"Popularity: {movie.popularity}");
                movieNode.Nodes.Add($"Vote Average: {movie.vote_average}");
                movieNode.Nodes.Add($"Vote Count: {movie.vote_count}");

                if (movie.genre_ids != null && movie.genre_ids.Count > 0)
                {
                    TreeNode genresNode = new TreeNode("Genres");
                    foreach (var genreId in movie.genre_ids)
                    {
                        genresNode.Nodes.Add(genreId.ToString());
                    }
                    movieNode.Nodes.Add(genresNode);
                }

                treeViewMovies.Nodes.Add(movieNode);
            }
        }

        private double[] GetSelectedPlayerStats(DataGridViewRow selectedRow)
        {
            return new double[]
            {
                SafeConvert(selectedRow.Cells["Match Played"].Value),
                SafeConvert(selectedRow.Cells["Goals"].Value),
                SafeConvert(selectedRow.Cells["Assists"].Value),
                SafeConvert(selectedRow.Cells["Penalty Kicks Made"].Value),
                SafeConvert(selectedRow.Cells["Non-Penalty Goals"].Value)
            };
        }

        private double SafeConvert(object value)
        {
            if (value == null || !double.TryParse(value.ToString(), out double result))
            {
                return 0.0; // Devuelve 0 si el valor es nulo o no es un número válido
            }
            return result;
        }

        private void DrawRadarPlot(double[] stats, string playerName)
        {
            graphic.Plot.Clear();
            var radar = graphic.Plot.Add.Radar(new double[][] { stats });

            string[] spokeLabels = { "Match Played", "Goals", "Assists", "Penalty Kicks Made", "Non-Penalty Goals" };
            radar.PolarAxis.SetSpokes(spokeLabels, length: 30);
            radar.PolarAxis.StraightLines = true;

            graphic.Plot.Title("Player Statistics");
            graphic.Refresh();
        }

        private void dgvData_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count > 0 && dataSourceType != "API" && dataSourceType != "DATABASE")
            {
                DataGridViewRow selectedRow = dgvData.SelectedRows[0];

                // Verificar que la fila no sea un encabezado o vacía
                if (selectedRow.IsNewRow) return;

                // Obtener estadísticas del jugador
                double[] stats = GetSelectedPlayerStats(selectedRow);

                // Obtener nombre del jugador
                string playerName = selectedRow.Cells["Player"].Value?.ToString() ?? "Player";

                // Dibujar gráfico
                DrawRadarPlot(stats, playerName);
            }
        }

        private void LoadPlayerStatsIntoTreeView(DataView dv)
        {
            DataTable dataTable = dv.ToTable();
            treeViewMovies.Nodes.Clear();

            var seasons = dataTable.AsEnumerable()
                .Skip(1) // Saltar encabezado
                .Where(r => !string.IsNullOrWhiteSpace(r.Field<string>("Season")))
                .GroupBy(row => row.Field<string>("Season"))
                .OrderBy(g => g.Key);

            foreach (var seasonGroup in seasons)
            {
                TreeNode seasonNode = new TreeNode($"Season: {seasonGroup.Key}");
                treeViewMovies.Nodes.Add(seasonNode);

                var leagues = seasonGroup.GroupBy(r => r.Field<string>("League") ?? "Liga desconocida");

                foreach (var leagueGroup in leagues)
                {
                    TreeNode leagueNode = new TreeNode($"League: {leagueGroup.Key}");
                    seasonNode.Nodes.Add(leagueNode);

                    var teams = leagueGroup.GroupBy(r => r.Field<string>("Team") ?? "Equipo desconocido");

                    foreach (var teamGroup in teams)
                    {
                        TreeNode teamNode = new TreeNode($"Team: {teamGroup.Key}");
                        leagueNode.Nodes.Add(teamNode);

                        var players = teamGroup.GroupBy(r => r.Field<string>("Player") ?? "Jugador desconocido");

                        foreach (var playerGroup in players)
                        {
                            TreeNode playerNode = new TreeNode($"Player: {playerGroup.Key}");
                            teamNode.Nodes.Add(playerNode);
                            foreach (var row in playerGroup)
                            {
                                TreeNode matchPlayedNode = new TreeNode($"Matches Played: {row["Match Played"]}");
                                TreeNode goalsNode = new TreeNode($"Goals: {row["Goals"]}");
                                TreeNode assistsNode = new TreeNode($"Assists: {row["Assists"]}");
                                TreeNode yellowCardsNode = new TreeNode($"Yellow Cards: {row["Yellow Cards"]}");
                                TreeNode redCardsNode = new TreeNode($"Red Cards: {row["Red Cards"]}");


                                playerNode.Nodes.Add(matchPlayedNode);
                                playerNode.Nodes.Add(goalsNode);
                                playerNode.Nodes.Add(assistsNode);
                                playerNode.Nodes.Add(yellowCardsNode);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateGraph()
        {
            graphic.Plot.Clear();
            graphic.Plot.ShowAxesAndGrid();
            var federationCounts = new Dictionary<string, int>();

            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (row.IsNewRow) continue;

                var titleCell = row.Cells["Title"].Value;
                var fedCell = row.Cells["Federation"].Value?.ToString() ?? "Unknown";

                string title = titleCell?.ToString() ?? "";

                if (title == "GM")
                {
                    if (!federationCounts.ContainsKey(fedCell))
                        federationCounts[fedCell] = 0;

                    federationCounts[fedCell]++;
                }
            }

            var top10Federations = federationCounts
                .OrderByDescending(kvp => kvp.Value) // Ordenar por cantidad descendente
                .Take(10)                           // Tomar los primeros 10
                .ToList();

            // Extraer las etiquetas y valores
            var labels = top10Federations.Select(kvp => kvp.Key).ToArray();
            var counts = top10Federations.Select(kvp => (double)kvp.Value).ToArray();

            // Paso 3: Crear barras horizontales
            ScottPlot.Bar[] bars = new ScottPlot.Bar[counts.Length];
            for (int i = 0; i < counts.Length; i++)
            {
                bars[i] = new ScottPlot.Bar
                {
                    Position = i + 1, // Posición de la barra
                    Value = counts[i], // Valor de la barra
                };
            }

            var barPlot = graphic.Plot.Add.Bars(bars);
            barPlot.Horizontal = true; // Configurar como gráfica horizontal

            // Configurar etiquetas del eje Y
            double[] positions = Enumerable.Range(1, labels.Length).Select(x => (double)x).ToArray(); // Posiciones numéricas
            graphic.Plot.Axes.Left.SetTicks(positions, labels);

            graphic.Plot.YLabel("Federation");
            graphic.Plot.XLabel("Number of Grandmasters");
            graphic.Plot.Axes.Margins(left: 0);
            graphic.Plot.Axes.SetLimitsX(0, counts.Max() * 1.1);
            // Actualizar gráfico
            graphic.Refresh();
        }

        private void LoadPlayerChessStatsIntoTreeView(DataView dv)
        {
            // Convertir el DataView a DataTable
            DataTable dataTable = dv.ToTable();

            // Limpiar los nodos del TreeView
            treeViewMovies.Nodes.Clear();

            // Agrupar los datos por Federation
            var federations = dataTable.AsEnumerable()
                .Where(r => !string.IsNullOrWhiteSpace(r.Field<string>("Federation")))
                .GroupBy(row => row.Field<string>("Federation"))
                .OrderBy(g => g.Key);

            // Iterar sobre cada grupo de federaciones
            foreach (var federationGroup in federations)
            {
                // Crear un nodo para la federación
                TreeNode federationNode = new TreeNode($"Federation: {federationGroup.Key}");
                treeViewMovies.Nodes.Add(federationNode);

                // Iterar sobre los jugadores dentro de la federación
                foreach (var row in federationGroup)
                {
                    // Crear un nodo para el jugador
                    TreeNode playerNode = new TreeNode($"{row["Name"]} ({row["Fide_id"]})");
                    federationNode.Nodes.Add(playerNode);

                    // Agregar detalles del jugador como subnodos
                    TreeNode genderNode = new TreeNode($"Gender: {row["Gender"]}");
                    TreeNode yearOfBirthNode = new TreeNode($"Year of Birth: {row["Year_of_birth"]}");
                    TreeNode titleNode = new TreeNode($"Title: {row["Title"]}");
                    TreeNode standardRatingNode = new TreeNode($"Standard Rating: {row["Standard_Rating"]}");
                    TreeNode rapidRatingNode = new TreeNode($"Rapid Rating: {row["Rapid_rating"]}");
                    TreeNode blitzRatingNode = new TreeNode($"Blitz Rating: {row["Blitz_rating"]}");
                    TreeNode inactiveFlagNode = new TreeNode($"Inactive Flag: {row["Inactive_flag"]}");

                    // Agregar los detalles al nodo del jugador
                    playerNode.Nodes.Add(genderNode);
                    playerNode.Nodes.Add(yearOfBirthNode);
                    playerNode.Nodes.Add(titleNode);
                    playerNode.Nodes.Add(standardRatingNode);
                    playerNode.Nodes.Add(rapidRatingNode);
                    playerNode.Nodes.Add(blitzRatingNode);
                    playerNode.Nodes.Add(inactiveFlagNode);
                }
            }
        }

        private void btnShowTreeview_Click(object sender, EventArgs e)
        {
            switch (dataSourceType)
            {
                case "API":
                    List<Movie> movies = ConvertDataTableToMovies(originalDataTable);
                    LoadMoviesIntoTreeView(movies);
                    break;
                case "CSV":
                case "XML":
                case "JSON":
                case "TXT":
                    LoadPlayerStatsIntoTreeView(new DataView(originalDataTable));
                    break;

                case "DATABASE":
                    LoadPlayerChessStatsIntoTreeView(new DataView(originalDataTable));
                    break;

                default:
                    MessageBox.Show("No data source selected.");
                    break;
            }
        }

        private async void cmbMovieType_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadMoviesByTypeAsync(cmbMovieType.SelectedItem?.ToString());
        }

    }
}
