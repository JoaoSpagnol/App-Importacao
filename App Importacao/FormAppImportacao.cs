using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace App_Importacao
{
    public partial class FormAppImportacao : Form
    {
        string connStr = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
        public FormAppImportacao()
        {
            InitializeComponent();
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            try
            {
              

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    //
                    // Open the SqlConnection.
                    //
                    con.Open();
                    //
                    // This code uses an SqlCommand based on the SqlConnection.
                    //
                    using (SqlCommand command = new SqlCommand("SELECT * from planilha", con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        var apirest = new RestAPI("http://developer.smmind.com.br/faina/wp-json/wc/v3/", "ck_d53f726ac27e7eeaa4ad4fba1d19cc7d73c98a43", "cs_e9fba980112026e7b451875af9acf2071f76dc55");
                        var wc = new WCObject(apirest); 

                        while (reader.Read())
                        {
                            var produtoadd = new Product
                            {

                                //options ....
                                type = "simple",
                                manage_stock = true,
                                name = reader["nomeProduto"].ToString(),
                                description = reader["html"].ToString(),
                                sku = reader["referencia"].ToString(),
                                price = Convert.ToDecimal(reader["preco"].ToString()),
                                regular_price = Convert.ToDecimal(reader["preco"].ToString()),
                                sale_price = Convert.ToDecimal(reader["preco"].ToString()),
                                images = BuscaImagem(Convert.ToInt32(reader["id"].ToString())),
                                stock_quantity = Convert.ToInt32(45)
                                
                                //variations=varis,
                            };

                           var produtoadred = wc.Product.Add(produtoadd).Result;
                        }
                    }
                }
            

            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Tivemos um erro no metodo btnIniciar_Click:" + ex.Message );
            }
        }

        private List<ProductImage> BuscaImagem(int? pkProduto)
        {
            var retorno = new List<ProductImage>();

            try
            {

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("select * from PlanilhaFoto where entity_id=" + pkProduto, con))
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var imagem = new ProductImage
                                {
                                
                                    src = dr["foto"].ToString(),
                                };
                                retorno.Add(imagem);
                            }
                        }
                    }

                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return retorno;
        }
    }
}
