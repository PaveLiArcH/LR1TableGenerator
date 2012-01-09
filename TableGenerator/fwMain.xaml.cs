using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace TableGenerator
{
	/// <summary>
	/// Логика взаимодействия для fwMain.xaml
	/// </summary>
	public partial class fwMain
	{
		DataTable cf_tblResult = null;
		cParser cf_parser = null;
		cLRTableGenerator cf_generator = new cLRTableGenerator();

		public fwMain()
		{
			InitializeComponent();
		}

		private void cm_showStatus(string a_text, bool a_showMessageBox)
		{
			f_tsslStatus.Content = a_text;
			if (a_showMessageBox)
				MessageBox.Show(a_text);
		}

		private void cm_showGram(cNotLL1Exception _ex)
		{
			f_rtbGram.Document.Blocks.Clear();
			//Color _col = f_rtbGram.SelectionColor;
			foreach (cLexem _lexem in cf_generator.cp_Lexems)
			{
				foreach (cProduction _production in _lexem.cp_ListProducts)
				{
					cm_addLexemToRichText(_lexem, f_rtbGram);
					f_rtbGram.AppendText("-> ");
					bool _first = true;
					foreach (cLexem _rightLex in _production.cp_RightPart)
					{
						if (_ex != null && _first && _lexem == _ex.cf_LeftLexem)
						{
							if (_rightLex.cp_Type == eLexType.NonTerminal || _rightLex == _ex.cf_Lexem)
							{
								//f_rtbGram.SelectionColor = Color.Red;
							}
						}
						if (_first && !_rightLex.cp_HasEpsilonProduct)
							_first = false;
						cm_addLexemToRichText(_rightLex, f_rtbGram);
						//f_rtbGram.SelectionColor = _col;
					}
					foreach (cLexem _action in _production.cp_ActionList)
					{
						cm_addLexemToRichText(_action, f_rtbGram);
					}
					f_rtbGram.AppendText("\n");
				}
			}
		}

		//private void cm_showGram(cParserException _ex)
		//{
		//    f_rtbGram.Document.Blocks.Clear();
		//    foreach (cToken _token in _ex.cf_Tokens)
		//    {
		//        switch (_token.cf_Type)
		//        {
		//            case eTokenType.перевод_строки:
		//                f_rtbGram.AppendText(Environment.NewLine);
		//                break;
		//            case eTokenType.стрелка:
		//                f_rtbGram.AppendText(" -> ");
		//                break;
		//            case eTokenType.Null:
		//                f_rtbGram.AppendText(" -> ");
		//                break;
		//            default:
		//                cm_addLexemToRichText(_token.cf_Value as cLexem, f_rtbGram);
		//                break;
		//        }
		//    }
		//}

		private void cm_addLexemToRichText(cLexem a_lexem, RichTextBox a_rtb)
		{
			if (a_lexem.cp_Type == eLexType.NonTerminal)
				a_rtb.AppendText("<");
			else if (a_lexem.cp_Type == eLexType.Action)
				a_rtb.AppendText("{");
			//a_rtb.SelectionFont = new Font(a_rtb.SelectionFont, FontStyle.Bold);
			//a_rtb.AppendText(a_lexem.ToString());
			TextRange _tr = new TextRange(a_rtb.Document.ContentEnd,a_rtb.Document.ContentEnd);
			_tr.Text = a_lexem.ToString();
			_tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
			_tr = new TextRange(a_rtb.Document.ContentEnd, a_rtb.Document.ContentEnd);
			_tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
			//a_rtb.SelectionFont = new Font(a_rtb.SelectionFont, FontStyle.Regular);
			if (a_lexem.cp_Type == eLexType.NonTerminal)
				a_rtb.AppendText(">");
			else if (a_lexem.cp_Type == eLexType.Action)
				a_rtb.AppendText("}");
			a_rtb.AppendText(" ");
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TextOptions.SetTextFormattingMode(f_rtbGram, TextFormattingMode.Ideal);
		}

		private void f_btnBrowseFileGram_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog _fileDialog = new OpenFileDialog();
			_fileDialog.AddExtension = true;
			_fileDialog.CheckFileExists = true;
			_fileDialog.CheckPathExists = true;
			_fileDialog.DereferenceLinks = true;
			_fileDialog.Filter = "Поддерживаемые файлы(*.xml, *.txt)|*.xml;*.txt|XML-файлы(*.xml)|*.xml|Текстовые файлы(*.txt)|*.txt";
			_fileDialog.FilterIndex = 0;
			_fileDialog.Multiselect = false;
			_fileDialog.RestoreDirectory = false;
			_fileDialog.Title = "Загрузить грамматику";
			_fileDialog.ValidateNames = true;
			if (_fileDialog.ShowDialog() == true)
			{
				f_txtFileGram.Text = _fileDialog.FileName;
			}
		}

		private void f_btnLoadGram_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(f_txtFileGram.Text))
			{
				#if DEBUG
				#else
				try
				#endif
				{
					cScanner _scanner = null;
					if (f_txtFileGram.Text.EndsWith(".xml"))
					{
						_scanner = new cXMLScanner(f_txtFileGram.Text);
					}
					else if (f_txtFileGram.Text.EndsWith(".txt"))
					{
						_scanner = new cTextScanner(f_txtFileGram.Text);
					}

					if (_scanner != null)
					{
						f_gbStep3.IsEnabled = false;
						f_btnViewTable.IsEnabled = false;
						f_gbStep4.IsEnabled = false;
						f_rtbGram.Document.Blocks.Clear();

						cf_parser = new cParser(_scanner, System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\metagram.xml");
						cf_parser.cm_Parse();
						cf_generator.cm_Init(cf_parser);
						_scanner.Dispose();

						// Вывод текста грамматики
						cm_showGram(null as cNotLL1Exception);
						f_gbStep3.IsEnabled = true;
						f_btnViewTable.IsEnabled = false;
						f_gbStep4.IsEnabled = false;
						cm_showStatus("Загрузка грамматики завершена.", false);
					}
					else
					{
						cm_showStatus("Неподдерживаемый формат файла.", true);
					}
				}
#if DEBUG
#else
				catch (cNotLL1Exception _ex)
                {
                    cm_showGram(_ex);
                    cm_showStatus(_ex.Message, true);
                }
                catch (cParserException _ex)
                {
                    cm_showGram(_ex);
                    cm_showStatus(_ex.Message, true);
                }
                catch (Exception _ex)
                {
                    cm_showStatus(_ex.Message, true);
                }
#endif
			}
			else
			{
				cm_showStatus("Выбранного файла не существует.", true);
			}
		}

		private void f_btnGenerateTable_Click(object sender, RoutedEventArgs e)
		{
#if DEBUG
#else
			try
#endif
			{
				cf_tblResult = cf_generator.cm_GenerateTable();
				f_btnViewTable.IsEnabled = true;
				f_gbStep4.IsEnabled = true;
				cm_showStatus("Генерация таблицы разбора завершена.", false);
			}
#if DEBUG
#else
			catch (cNotLL1Exception _ex)
			{
				// Вывод текста грамматики, который загружен целиком к данному моменту
				cm_showGram(_ex);
				cm_showStatus(_ex.Message, true);
			}
			catch (Exception _ex)
			{
				cm_showStatus(_ex.Message, true);
			}
#endif
		}

		private void f_btnViewTable_Click(object sender, RoutedEventArgs e)
		{
			(new fShowTable(cf_tblResult)).ShowDialog();
		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog _fileDialog = new SaveFileDialog();
			_fileDialog.AddExtension = true;
			_fileDialog.CheckFileExists = false;
			_fileDialog.CheckPathExists = true;
			_fileDialog.DereferenceLinks = true;
			_fileDialog.Filter = "XML-файлы(*.xml)|*.xml";
			_fileDialog.FilterIndex = 0;
			_fileDialog.OverwritePrompt = true;
			_fileDialog.RestoreDirectory = false;
			_fileDialog.Title = "Сохранить таблицу разбора как";
			_fileDialog.ValidateNames = true;
			if (_fileDialog.ShowDialog() == true)
			{
				f_txtFileTable.Text = _fileDialog.FileName;
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				if (f_txtFileTable.Text.EndsWith(".xml"))
				{
					cFileTableWriter.cm_SaveXML(f_txtFileTable.Text, cf_tblResult);
					cm_showStatus("Сохранение успешно завершено.", true);
				}
				else
				{
					cm_showStatus("Неподдерживаемый формат файла.", true);
				}
			}
			catch (Exception _ex)
			{
				cm_showStatus("Ошибка при сохранении: " + _ex.Message, true);
			}
		}

		private void f_rtbGram_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			
		}

		private void f_rtbGram_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			KeyStates _LCtrlState = Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down;
			KeyStates _RCtrlState = Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down;
			if ((_LCtrlState == KeyStates.Down) || (_RCtrlState == KeyStates.Down))
			{
				if (e.Delta > 0)
				{
					f_rtbScale.Value += 0.5;
				}
				else
				{
					f_rtbScale.Value -= 0.5;
				}
				e.Handled = true;
			}
		}
	}
}
