﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Decryptor
{
    public partial class FormMain : Form
    {
        static byte[] aeskey = new byte[] { 205, 248, 81, 248, 239, 204, 156, 88, 201, 128, 100, 46, 203, 67, 102, 92, 27, 55, 121, 117, 78, 34, 70, 44, 11, 246, 60, 214, 100, 41, 205, 201 };

        public FormMain()
        {
            InitializeComponent();
        }

        string GetDecrypted(string text)
        {
            /*!
	            \brief Расшифровка лога
                \param text Лог
                \return Расшифрованный лог
            */
            string output = string.Empty;
            foreach (string line in text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                if (line != "ENDBLOCK")
                    try
                    {
                        output += FromAes256(line) + "\n";
                    }
                    catch (Exception e)
                    {
                        return "Error";
                    }
            return output;
        }

        byte[] hexToBytes(string hex)
        {
            /*!
	            \brief Перевод из hex-строки в массив байт
                \param text Входная строка
                \return Массив байт
            */
            byte[] byteArray = new byte[hex.Length / 2];
            for (int i = 0; i < byteArray.Length; i++)
            {
                string strbyte = hex.Substring(i * 2, 2);
                byteArray[i] = byte.Parse(strbyte, System.Globalization.NumberStyles.HexNumber);
            }
            return byteArray;
        }

        string FromAes256(string cryptedText)
        {
            /*!
	            \brief Расшифровка строки aes256
                \param cryptedText Зашифрованная строка
                \return Расшифрованная строка
            */
            byte[] cryptedByteArray = hexToBytes(cryptedText);
            byte[] bytesIV = new byte[16];
            byte[] mess = new byte[cryptedByteArray.Length - 16];
            
            for (int i = cryptedByteArray.Length - 16, j = 0; i < cryptedByteArray.Length; i++, j++)
                bytesIV[j] = cryptedByteArray[i]; //Списываем соль
            for (int i = 0; i < cryptedByteArray.Length - 16; i++)
                mess[i] = cryptedByteArray[i]; //Списываем оставшуюся часть сообщения

            Aes aes = Aes.Create();
            aes.Key = aeskey;
            aes.IV = bytesIV;

            string text;
            byte[] data = mess;
            ICryptoTransform crypt = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(data))
            using (CryptoStream cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
                text = sr.ReadToEnd();
            return text;
        }
        
        string GetText(string input)
        {
            /*!
	            \brief Получение набранного текста из лога
                \param input Лог
                \return Набранный текст
            */
            char[] raw = input.ToCharArray();
            string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            string result = String.Empty;
            for (int i = 0; i < raw.Length; i++)
                if (alphabet.Contains(raw[i].ToString()))
                    result += raw[i];
                else if (input.Substring(i).StartsWith("Space"))
                    result += " ";
            result = result.ToLower();
            return result;
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку открытия файла
            */
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textView.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
                textView.Invalidate();
            }
        }

        private void keyButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку смены ключа шифрования
            */
            FormKey formKey = new FormKey();
            formKey.ShowDialog();
            if (formKey.key != null)
            {
                if (formKey.key.Length == 64)
                {
                    try
                    {
                        aeskey = hexToBytes(formKey.key);
                    }
                    catch
                    {
                        MessageBox.Show("Key has wrong format");
                    }
                }
                else MessageBox.Show("Key has wrong length");
            }
        }

        private void cleanButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку очистки экрана
            */
            textView.Text = string.Empty;
            textView.Invalidate();
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку дешифрования
            */
            textView.Text = GetDecrypted(textView.Text);
            textView.Invalidate();
        }

        private void textButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку получения набранного текста
            */
            textView.Text = GetText(textView.Text);
            textView.Invalidate();
        }

        private void doubleButton_Click(object sender, EventArgs e)
        {
            /*!
	            \brief Обработчик нажатия на кнопку дешифрования и получения набранного текста
            */
            textView.Text = GetText(GetDecrypted(textView.Text));
            textView.Invalidate();
        }
    }
}
