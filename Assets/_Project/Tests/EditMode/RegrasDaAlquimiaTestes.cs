using System;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaAlquimiaTestes
    {
        static readonly DateTime Agora = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public void Pocao_dura_os_minutos_dela()
        {
            var forca = new DefinicaoDePocao { DuracaoEmMinutos = 10 };
            long fim = RegrasDaAlquimia.NovoFim(forca, 0, Agora);
            Assert.IsTrue(RegrasDaAlquimia.Ativa(fim, Agora.AddMinutes(9)));
            Assert.IsFalse(RegrasDaAlquimia.Ativa(fim, Agora.AddMinutes(10)));
        }

        [Test]
        public void Beber_de_novo_soma_o_tempo_que_faltava()
        {
            var forca = new DefinicaoDePocao { DuracaoEmMinutos = 10 };
            long fim = RegrasDaAlquimia.NovoFim(forca, 0, Agora);
            long outra = RegrasDaAlquimia.NovoFim(forca, fim, Agora.AddMinutes(4));
            Assert.AreEqual(Agora.AddMinutes(20).Ticks, outra);
        }

        [Test]
        public void Pocao_sem_duracao_vale_ate_ser_usada()
        {
            var sono = new DefinicaoDePocao { DuracaoEmMinutos = 0 };
            long fim = RegrasDaAlquimia.NovoFim(sono, 0, Agora);
            Assert.IsTrue(RegrasDaAlquimia.Ativa(fim, Agora.AddDays(30)));
        }
    }
}
