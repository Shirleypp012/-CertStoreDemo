using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace CertStoreDemo
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private readonly Button btnCreateCert = new Button();
        private readonly Button btnReadCert = new Button();
        private readonly Button btnExportPfx = new Button();
        private readonly Button btnReadPfx = new Button();
        private readonly Button btnEncrypt = new Button();
        private readonly Button btnDecrypt = new Button();
        private readonly Button btnDeleteCert = new Button();

        private readonly RichTextBox richTextBox2 = new RichTextBox();
        private readonly RichTextBox richTextBox3 = new RichTextBox();
        private readonly RichTextBox richTextBox4 = new RichTextBox();
        private readonly TextBox txtPlain = new TextBox();
        private readonly Label lblPlain = new Label();

        private const string CertSubject = "CN=CursorCertDemoUser";
        private const string CertFriendlyName = "CursorCertDemoCert";
        private const string PfxPassword = "123456";
        private readonly string pfxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CursorCertDemo.pfx");

        private string certThumbprint = string.Empty;
        private byte[] lastCipher;

        public MainForm()
        {
            Text = "数字证书程序";
            Width = 820;
            Height = 430;
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
            HookEvents();
        }

        private void BuildLayout()
        {
            btnCreateCert.Text = "1. 生成证书";
            btnCreateCert.SetBounds(20, 20, 130, 40);

            btnReadCert.Text = "2. 读取证书";
            btnReadCert.SetBounds(20, 70, 130, 40);

            btnExportPfx.Text = "3. 导出pfx文件";
            btnExportPfx.SetBounds(20, 120, 130, 40);

            btnReadPfx.Text = "4. 读取pfx文件";
            btnReadPfx.SetBounds(170, 20, 130, 40);

            richTextBox2.SetBounds(310, 20, 480, 120);

            lblPlain.Text = "明文字符串：";
            lblPlain.SetBounds(170, 150, 90, 24);

            txtPlain.SetBounds(260, 148, 530, 24);
            txtPlain.Text = "Hello Certificate RSA";

            btnEncrypt.Text = "5. 公钥加密";
            btnEncrypt.SetBounds(170, 180, 130, 40);

            richTextBox3.SetBounds(310, 180, 480, 90);

            btnDecrypt.Text = "6. 私钥解密";
            btnDecrypt.SetBounds(170, 280, 130, 40);

            richTextBox4.SetBounds(310, 280, 480, 90);

            btnDeleteCert.Text = "7. 删除存储区证书";
            btnDeleteCert.SetBounds(20, 330, 130, 40);

            Controls.Add(btnCreateCert);
            Controls.Add(btnReadCert);
            Controls.Add(btnExportPfx);
            Controls.Add(btnReadPfx);
            Controls.Add(btnEncrypt);
            Controls.Add(btnDecrypt);
            Controls.Add(btnDeleteCert);
            Controls.Add(richTextBox2);
            Controls.Add(richTextBox3);
            Controls.Add(richTextBox4);
            Controls.Add(txtPlain);
            Controls.Add(lblPlain);
        }

        private void HookEvents()
        {
            btnCreateCert.Click += BtnCreateCert_Click;
            btnReadCert.Click += BtnReadCert_Click;
            btnExportPfx.Click += BtnExportPfx_Click;
            btnReadPfx.Click += BtnReadPfx_Click;
            btnEncrypt.Click += BtnEncrypt_Click;
            btnDecrypt.Click += BtnDecrypt_Click;
            btnDeleteCert.Click += BtnDeleteCert_Click;
        }

        private void BtnCreateCert_Click(object sender, EventArgs e)
        {
            try
            {
                RemoveOldCertsBySubject(CertSubject);

                string script = "$cert = New-SelfSignedCertificate -Subject '" + CertSubject +
                                "' -CertStoreLocation 'Cert:\\CurrentUser\\My' -FriendlyName '" + CertFriendlyName +
                                "' -KeyExportPolicy Exportable -KeyAlgorithm RSA -KeyLength 2048 " +
                                "-Provider 'Microsoft Enhanced RSA and AES Cryptographic Provider';" +
                                "[Console]::Write($cert.Thumbprint)";
                string output = RunPowerShell(script).Trim();
                if (string.IsNullOrWhiteSpace(output))
                {
                    throw new Exception("未获取到新证书Thumbprint。");
                }

                certThumbprint = output.Replace(" ", "").ToUpperInvariant();
                MessageBox.Show("证书创建成功并保存到 CurrentUser\\My。\r\nThumbprint: " + certThumbprint);
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成证书失败：" + ex.Message);
            }
        }

        private void BtnReadCert_Click(object sender, EventArgs e)
        {
            try
            {
                X509Certificate2 cert = GetCertFromStore();
                if (cert == null)
                {
                    MessageBox.Show("存储区未找到目标证书，请先点击“1.生成证书”。");
                    return;
                }

                certThumbprint = cert.Thumbprint;
                var sb = new StringBuilder();
                sb.AppendLine("证书主题: " + cert.Subject);
                sb.AppendLine("颁发者: " + cert.Issuer);
                sb.AppendLine("序列号: " + cert.SerialNumber);
                sb.AppendLine("生效时间: " + cert.NotBefore);
                sb.AppendLine("失效时间: " + cert.NotAfter);
                sb.AppendLine("Thumbprint: " + cert.Thumbprint);
                sb.AppendLine("含私钥: " + cert.HasPrivateKey);
                MessageBox.Show(sb.ToString(), "存储区证书信息");
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取证书失败：" + ex.Message);
            }
        }

        private void BtnExportPfx_Click(object sender, EventArgs e)
        {
            try
            {
                X509Certificate2 cert = GetCertFromStore();
                if (cert == null)
                {
                    MessageBox.Show("存储区未找到目标证书，请先生成证书。");
                    return;
                }

                byte[] pfxBytes = cert.Export(X509ContentType.Pfx, PfxPassword);
                File.WriteAllBytes(pfxPath, pfxBytes);
                MessageBox.Show("导出成功：\r\n" + pfxPath + "\r\n密码: " + PfxPassword);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出pfx失败：" + ex.Message);
            }
        }

        private void BtnReadPfx_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(pfxPath))
                {
                    MessageBox.Show("未找到pfx文件，请先执行“3.导出pfx文件”。");
                    return;
                }

                var cert = new X509Certificate2(
                    pfxPath,
                    PfxPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                var sb = new StringBuilder();
                sb.AppendLine("PFX路径: " + pfxPath);
                sb.AppendLine("主题: " + cert.Subject);
                sb.AppendLine("Thumbprint: " + cert.Thumbprint);
                sb.AppendLine("含私钥: " + cert.HasPrivateKey);
                sb.AppendLine();
                sb.AppendLine("公钥信息(XML):");
                var pub = GetPublicRsaProvider(cert);
                if (pub != null)
                {
                    sb.AppendLine(pub.ToXmlString(false));
                }
                else
                {
                    sb.AppendLine("读取公钥失败。");
                }

                sb.AppendLine();
                sb.AppendLine("私钥信息(XML):");
                var pri = GetPrivateRsaProvider(cert);
                if (pri != null)
                {
                    sb.AppendLine(pri.ToXmlString(true));
                }
                else
                {
                    sb.AppendLine("读取私钥失败。");
                }

                richTextBox2.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取pfx失败：" + ex.Message);
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(pfxPath))
                {
                    MessageBox.Show("未找到pfx文件，请先导出并读取pfx。");
                    return;
                }

                string plain = txtPlain.Text;
                if (string.IsNullOrEmpty(plain))
                {
                    MessageBox.Show("请输入明文字符串。");
                    return;
                }

                var cert = new X509Certificate2(pfxPath, PfxPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                var pub = GetPublicRsaProvider(cert);
                if (pub == null)
                {
                    MessageBox.Show("读取公钥失败。");
                    return;
                }

                byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
                lastCipher = pub.Encrypt(plainBytes, false);
                richTextBox3.Text = Convert.ToBase64String(lastCipher);
            }
            catch (Exception ex)
            {
                MessageBox.Show("公钥加密失败：" + ex.Message);
            }
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (lastCipher == null || lastCipher.Length == 0)
                {
                    MessageBox.Show("请先执行“5.公钥加密”。");
                    return;
                }

                if (!File.Exists(pfxPath))
                {
                    MessageBox.Show("未找到pfx文件。");
                    return;
                }

                var cert = new X509Certificate2(
                    pfxPath,
                    PfxPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                var pri = GetPrivateRsaProvider(cert);
                if (pri == null)
                {
                    MessageBox.Show("读取私钥失败。");
                    return;
                }

                byte[] decrypted = pri.Decrypt(lastCipher, false);
                string plain = Encoding.UTF8.GetString(decrypted);
                richTextBox4.Text = plain;
            }
            catch (Exception ex)
            {
                MessageBox.Show("私钥解密失败：" + ex.Message);
            }
        }

        private void BtnDeleteCert_Click(object sender, EventArgs e)
        {
            try
            {
                int count = RemoveOldCertsBySubject(CertSubject);
                certThumbprint = string.Empty;
                MessageBox.Show("删除完成，共删除 " + count + " 张证书。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除证书失败：" + ex.Message);
            }
        }

        private static int RemoveOldCertsBySubject(string subject)
        {
            int removed = 0;
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                var toRemove = new X509Certificate2Collection();
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    if (string.Equals(cert.Subject, subject, StringComparison.OrdinalIgnoreCase))
                    {
                        toRemove.Add(cert);
                    }
                }

                if (toRemove.Count > 0)
                {
                    removed = toRemove.Count;
                    store.RemoveRange(toRemove);
                }
            }
            return removed;
        }

        private X509Certificate2 GetCertFromStore()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 best = null;
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    if (!string.Equals(cert.Subject, CertSubject, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (best == null || cert.NotBefore > best.NotBefore)
                    {
                        best = cert;
                    }
                }
                return best;
            }
        }

        private static string RunPowerShell(string command)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command.Replace("\"", "\\\"") + "\"";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    throw new Exception("无法启动 PowerShell。");
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(string.IsNullOrWhiteSpace(stderr) ? "PowerShell 执行失败。" : stderr);
                }
                return stdout;
            }
        }

        private static RSACryptoServiceProvider GetPublicRsaProvider(X509Certificate2 cert)
        {
            try
            {
                var provider = cert.PublicKey.Key as RSACryptoServiceProvider;
                if (provider != null)
                {
                    return provider;
                }

                var rsa = cert.PublicKey.Key as RSA;
                if (rsa == null)
                {
                    return null;
                }

                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(rsa.ExportParameters(false));
                return csp;
            }
            catch
            {
                return null;
            }
        }

        private static RSACryptoServiceProvider GetPrivateRsaProvider(X509Certificate2 cert)
        {
            try
            {
                var provider = cert.PrivateKey as RSACryptoServiceProvider;
                if (provider != null)
                {
                    return provider;
                }

                var rsa = cert.PrivateKey as RSA;
                if (rsa == null)
                {
                    return null;
                }

                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(rsa.ExportParameters(true));
                return csp;
            }
            catch
            {
                return null;
            }
        }
    }
}
