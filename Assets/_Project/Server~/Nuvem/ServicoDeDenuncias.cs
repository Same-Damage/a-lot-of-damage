using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;

namespace ALotOfDamage.Nuvem
{
    // Denúncias do chat, para a equipe ler no painel (Cloud Save, item privado "denuncias-<mês>")
    public class ServicoDeDenuncias
    {
        const int PorDia = 10;
        const string ChaveDoLimite = "denuncias";

        class Limite { public int Dia, Quantas; }
        class Registro { public string De = "", Contra = "", Canal = "", Texto = ""; public DateTime Quando; }

        readonly IArmazem armazem;
        readonly string eu;
        readonly DateTime agora;

        public ServicoDeDenuncias(IArmazem armazem, string eu, DateTime agora)
        {
            this.armazem = armazem;
            this.eu = eu;
            this.agora = agora;
        }

        public async Task<bool> Registrar(Denuncia d)
        {
            if (d == null || string.IsNullOrEmpty(d.JogadorId) || d.JogadorId == eu) return false;
            var (texto, trava) = await armazem.Ler("", ChaveDoLimite);
            var l = Recepcao.Ler<Limite>(texto) ?? new Limite();
            int dia = RegrasDaGuilda.Dia(agora);
            if (l.Dia != dia) l = new Limite { Dia = dia };
            if (l.Quantas >= PorDia) return false;
            l.Quantas++;
            if (!await armazem.Gravar("", ChaveDoLimite, Recepcao.Escrever(l), trava)) return false;
            var r = new Registro { De = eu, Contra = Cortar(d.JogadorId, 64), Canal = Cortar(d.Canal, 64), Texto = Cortar(d.Texto, RegrasDoChat.TamanhoMaximo), Quando = agora };
            return await armazem.Gravar($"denuncias-{agora:yyyyMM}", Guid.NewGuid().ToString("N"), Recepcao.Escrever(r), null);
        }

        static string Cortar(string s, int n) => s == null ? "" : s.Length <= n ? s : s.Substring(0, n);
    }
}
