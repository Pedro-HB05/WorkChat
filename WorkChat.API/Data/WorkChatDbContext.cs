using Microsoft.EntityFrameworkCore;
using WorkChat.Models;

namespace WorkChat.Data;

public sealed class WorkChatDbContext(DbContextOptions<WorkChatDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<UsuarioSetor> UsuarioSetores => Set<UsuarioSetor>();
    public DbSet<Conversa> Conversas => Set<Conversa>();
    public DbSet<Mensagem> Mensagens => Set<Mensagem>();
    public DbSet<TransferenciaConversa> TransferenciasConversa => Set<TransferenciaConversa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Companies");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Nome).HasColumnName("Name");
            entity.Property(x => x.MensagemBoasVindas).HasColumnName("WelcomeMessage");
            entity.Property(x => x.MensagemForaHorario).HasColumnName("OfflineMessage");
            entity.Property(x => x.MensagemEspera).HasColumnName("QueueMessage");
            entity.Property(x => x.LimiteChatsPadrao).HasColumnName("MaxChatsPerUser");
            entity.Property(x => x.Ativa).HasColumnName("IsActive");
            entity.Property(x => x.DataCriacao).HasColumnName("CreatedAt");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.EmpresaId).HasColumnName("CompanyId");
            entity.Property(x => x.Nome).HasColumnName("Name");
            entity.Property(x => x.Email).HasColumnName("Email");
            entity.Property(x => x.SenhaHash).HasColumnName("PasswordHash");
            entity.Property(x => x.Perfil).HasColumnName("Role");
            entity.Property(x => x.StatusAtendimento).HasColumnName("Status");
            entity.Property(x => x.LimiteChats).HasColumnName("MaxChats");
            entity.Property(x => x.UltimaDistribuicao).HasColumnName("LastAssignedAt");
            entity.Property(x => x.Ativo).HasColumnName("IsActive");
            entity.Property(x => x.DataCriacao).HasColumnName("CreatedAt");
            entity.HasOne(x => x.Empresa).WithMany(x => x.Usuarios).HasForeignKey(x => x.EmpresaId);
        });

        modelBuilder.Entity<Setor>(entity =>
        {
            entity.ToTable("Departments");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.EmpresaId).HasColumnName("CompanyId");
            entity.Property(x => x.Nome).HasColumnName("Name");
            entity.Property(x => x.Ativo).HasColumnName("IsActive");
            entity.HasOne(x => x.Empresa).WithMany(x => x.Setores).HasForeignKey(x => x.EmpresaId);
        });

        modelBuilder.Entity<UsuarioSetor>(entity =>
        {
            entity.ToTable("UserDepartments");
            entity.Property(x => x.UsuarioId).HasColumnName("UserId");
            entity.Property(x => x.SetorId).HasColumnName("DepartmentId");
            entity.HasKey(x => new { x.UsuarioId, x.SetorId });
            entity.HasOne(x => x.Usuario).WithMany(x => x.UsuarioSetores).HasForeignKey(x => x.UsuarioId);
            entity.HasOne(x => x.Setor).WithMany(x => x.UsuarioSetores).HasForeignKey(x => x.SetorId);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.EmpresaId).HasColumnName("CompanyId");
            entity.Property(x => x.Nome).HasColumnName("Name");
            entity.Property(x => x.Email).HasColumnName("Email");
            entity.Property(x => x.Telefone).HasColumnName("Phone");
            entity.Property(x => x.Prioridade).HasColumnName("Priority");
            entity.Property(x => x.Vip).HasColumnName("IsVip");
            entity.Property(x => x.DataCriacao).HasColumnName("CreatedAt");
            entity.HasOne(x => x.Empresa).WithMany(x => x.Clientes).HasForeignKey(x => x.EmpresaId);
        });

        modelBuilder.Entity<Conversa>(entity =>
        {
            entity.ToTable("Conversations");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.EmpresaId).HasColumnName("CompanyId");
            entity.Property(x => x.ClienteId).HasColumnName("CustomerId");
            entity.Property(x => x.SetorId).HasColumnName("DepartmentId");
            entity.Property(x => x.AtendenteId).HasColumnName("AssignedUserId");
            entity.Property(x => x.Status).HasColumnName("Status");
            entity.Property(x => x.Prioridade).HasColumnName("Priority");
            entity.Property(x => x.PosicaoFila).HasColumnName("QueuePosition");
            entity.Property(x => x.DataAbertura).HasColumnName("CreatedAt");
            entity.Property(x => x.DataInicioAtendimento).HasColumnName("AssignedAt");
            entity.Property(x => x.DataEncerramento).HasColumnName("ClosedAt");
            entity.HasOne(x => x.Empresa).WithMany(x => x.Conversas).HasForeignKey(x => x.EmpresaId);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Conversas).HasForeignKey(x => x.ClienteId);
            entity.HasOne(x => x.Setor).WithMany(x => x.Conversas).HasForeignKey(x => x.SetorId);
            entity.HasOne(x => x.Atendente).WithMany(x => x.ConversasAtendidas).HasForeignKey(x => x.AtendenteId);
        });

        modelBuilder.Entity<Mensagem>(entity =>
        {
            entity.ToTable("Messages");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.ConversaId).HasColumnName("ConversationId");
            entity.Property(x => x.RemetenteTipo).HasColumnName("SenderType");
            entity.Property(x => x.UsuarioId).HasColumnName("SenderUserId");
            entity.Property(x => x.ClienteId).HasColumnName("SenderCustomerId");
            entity.Property(x => x.Conteudo).HasColumnName("Content");
            entity.Property(x => x.Tipo).HasColumnName("MessageType");
            entity.Property(x => x.DataEnvio).HasColumnName("CreatedAt");
            entity.HasOne(x => x.Conversa).WithMany(x => x.Mensagens).HasForeignKey(x => x.ConversaId);
            entity.HasOne(x => x.Usuario).WithMany(x => x.Mensagens).HasForeignKey(x => x.UsuarioId);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Mensagens).HasForeignKey(x => x.ClienteId);
        });

        modelBuilder.Entity<TransferenciaConversa>(entity =>
        {
            entity.ToTable("ConversationTransfers");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.ConversaId).HasColumnName("ConversationId");
            entity.Property(x => x.AtendenteOrigemId).HasColumnName("FromUserId");
            entity.Property(x => x.AtendenteDestinoId).HasColumnName("ToUserId");
            entity.Property(x => x.SetorOrigemId).HasColumnName("FromDepartmentId");
            entity.Property(x => x.SetorDestinoId).HasColumnName("ToDepartmentId");
            entity.Property(x => x.Motivo).HasColumnName("Reason");
            entity.Property(x => x.DataTransferencia).HasColumnName("CreatedAt");
            entity.HasOne(x => x.Conversa).WithMany(x => x.Transferencias).HasForeignKey(x => x.ConversaId);
        });
    }
}
