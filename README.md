# TDV_EDJD_PJ2

- Astroidz Gaming


# Introdução:

- Este projeto teve por base a criação de um jogo com base no framework Monogame.


# Trabalho realizado por:

- 29579 - Roberto Alvarenga

- 29848 - André Azevedo

- 29580 - João Carvalho


# Sobre o Jogo:

- Este jogo ocorre em mundo aberto, onde a câmara segue o player, e o objetivo é tentar sobreviver o maior tempo possível e destruir os inimigos para obter score.

- Para jogar usa-se as teclas (wasd) e o rato para orientar a direção do player e o botão esquerdo para disparar. 


# Sobre o código:

- **Mundo**
	
	- O mundo é em plano aberto e não tem nenhum sprite associado, as estralas decorativas são geradas aleatóriamente no inicio do jogo.  

- **Classes:**

	- Game1.cs: Classe base da framework monogame e principal responsável pelo jogo em geral e pelo player. Para além de coisas essenciais como o controlo de gamestates (Menu, Playing, Quitting) e LoadContent, também é responsável por a criação e controlo do Player, e por gerar estrelas, asteroídes e inimigos aleatóriamente, e por atualizar os objetos definidos em outras classes e controlar a sua população (Evitando grandes quantidades de asteroídes/bullets). Também é aqui que se faz todo o draw, incluindo o UI. Adicionalmente responsável por um método de debug, e um métdo colisão SAT (Separating Axis Theorem) que são usadas em outras partes do código para todo o tipo de colisões.
	
	- Asteroid.cs: Nesta classe temos definido o objeto do asteroides, aqui é verificada a lógica de movimento e de colisão entre todos os objetos (excepto bullets) e asteroídes, e também a desativação de asteroides se eles estiverem demasiado afastados do player.
	
	- Bullet.cs: Nesta classe é definido o objeto Bulet, o controlo da sua posição/velocidade, e as suas colisões com outros objetos
	
	- Enemy.cs: Nesta classe temos definidos os objetos "inimigos", one está definido o construtor deste, o AI usando um método raycast para evitar colisões com asteroides, o disparo de bullets em direção ao player, e é responsável por verificar as colisões do inimigo com o player.
	
- **Bugs encontrados por resolver**

	- Fazer reset da lista de asteroides e iminigos quando o player morre.

# Melhorias:

- Aplicar uma textura 2D aos asteroides em vez de textura simples.

- Implementar a lista de scores.  


# Conclusão:

- Este projeto foi desafiante, uma vez que a framework monogame obriga a pensar com cuidado na sua implementação, pois não existe uma base como em outras frameworks do género unity. 

