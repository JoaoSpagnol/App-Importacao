using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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
            var int_variacao = 0;
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
                            var lstProdutoVariacao = BuscaVariacao(Convert.ToInt32(reader["id"].ToString()));

                            if (lstProdutoVariacao.Any())
                            {
                                if (lstProdutoVariacao.Count == 1)
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
                                        stock_quantity = Convert.ToInt32(reader["qty"].ToString()),

                                        //variations=varis,
                                    };

                                    var produtoadred = wc.Product.Add(produtoadd).Result;
                                }
                                else
                                {
                                    var produtoadd = new Product
                                    {

                                        //options ....
                                        type = "variable",
                                        manage_stock = true,
                                        name = reader["nomeProduto"].ToString(),
                                        description = reader["html"].ToString(),
                                        sku = reader["referencia"].ToString(),
                                        price = 0,

                                        attributes = new List<ProductAttributeLine>
                                 {
                                    new ProductAttributeLine
                                    {
                                        name = "Tamanho do Aro",
                                        
                                        options = BuscarLista(lstProdutoVariacao),
                                        visible = true,
                                        variation = true
                                    }
                                    
                                },
                                        images = BuscaImagem(Convert.ToInt32(reader["id"].ToString()))
                                        //variations=varis,
                                    };

                                    //Returns the new product with the id.
                                    produtoadd = wc.Product.Add(produtoadd).Result;


                                    if (lstProdutoVariacao.Any())
                                    {
                                        foreach (var itemvariacao in lstProdutoVariacao)
                                        {
                                            Variation var = new Variation
                                            {
                                                regular_price = Convert.ToDecimal(reader["preco"].ToString()),
                                                sku = reader["referencia"].ToString() + int_variacao,
                                                _virtual = false,
                                                stock_quantity = Convert.ToInt32(reader["qty"].ToString()),
                                                manage_stock = true,
                                                attributes = new List<VariationAttribute>
                                    {
                                        new VariationAttribute
                                        {
                                            name = "Tamanho do Aro",
                                            option =itemvariacao.Variacao
                                        }
                                    }
                                            };
                                            //Returns the new variation with the id
                                            var = wc.Product.Variations.Add(var, produtoadd.id.Value).Result;

                                            // Add the variation id to the product
                                            produtoadd.variations.Add(var.id.Value);

                                            //Update the product
                                            produtoadd = wc.Product.Update(produtoadd.id.Value, produtoadd).Result;

                                            int_variacao++;

                                        }
                                    }
                                }
                            }

                            
                          
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

        private List<ProdutoVariacao> BuscaVariacao(int? pkProduto)
        {
            var retorno = new List<ProdutoVariacao>();

            try
            {

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("select * from PlanilhaVariacao where produto=" + pkProduto, con))
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var prodvaricao = new ProdutoVariacao
                                {

                                    Variacao = dr["variacao"].ToString(),
                                };
                                retorno.Add(prodvaricao);
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


        /// <summary>
        /// Buscars the lista.
        /// </summary>
        /// <param name="lstProdutoVariacao">The LST produto variacao.</param>
        /// <returns></returns>
        private List<string> BuscarLista(List<ProdutoVariacao> lstProdutoVariacao)
        {
            var lststring = new List<string>();
            try
            {
                if (lstProdutoVariacao.Any())
                {
                    foreach (var produtoVariacao in lstProdutoVariacao)
                    {
                        lststring.Add(produtoVariacao.Variacao);
                    }

                    return lststring;
                }
            }
            catch (Exception e)
            {
                throw;
            }



            return new List<string>();
        }
    }
}
